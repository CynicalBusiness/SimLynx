using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SimLynx.Core.Phasing;

/// <summary>
/// A plan for executing SimLynx phases in a specific order.
/// </summary>
public class PhasePlan : IEnumerable<PhasePlan.Entry>
{
    /// <summary>
    /// An empty phase plan, with no entries.
    /// </summary>
    public static PhasePlan Empty { get; } = new([]);

    private PhasePlan(IEnumerable<Entry> entries)
    {
        Entries = [.. entries];
    }

    /// <summary>
    /// The current entries in the phase plan, in order of execution.
    /// </summary>
    public Entry[] Entries { get; }

    /// <summary>
    /// Attempts to get the <paramref name="nextEntry"/> and the next phase plan, returning whether or not there are
    /// further entries to execute. The <paramref name="nextPlan"/> is provided as a plan not containing the provided
    /// entry.
    /// </summary>
    /// <remarks>
    /// The <paramref name="nextPlan"/> is always provided, but may be the same as the current plan if there are no
    /// further entries to execute.
    /// </remarks>
    /// <param name="nextEntry">The next entry, if available; otherwise, null.</param>
    /// <param name="nextPlan">The next phase plan, if available; otherwise, the current plan.</param>
    /// <returns><c>true</c> if there is a next entry; otherwise, <c>false</c>.</returns>
    public bool TryNext([MaybeNullWhen(false)] out Entry nextEntry, out PhasePlan nextPlan)
    {
        if (Entries.Length == 0)
        {
            nextEntry = null;
            nextPlan = this;
            return false;
        }
        else
        {
            nextEntry = Entries[0];
            nextPlan = new PhasePlan(Entries[1..]);
            return true;
        }
    }

    /// <summary>
    /// Creates a new phase plan that includes all entries up to (and including, if <paramref name="inclusive"/>), the
    /// entry with the specified phase ID. If no entry with the specified ID exists, an exception is thrown.
    /// </summary>
    /// <param name="phaseType">The type of the phase to stop at.</param>
    /// <param name="inclusive"><c>true</c> to include the specified phase; <c>false</c> to exclude it.</param>
    /// <returns>A new <see cref="PhasePlan"/> instance containing the specified range of entries.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public PhasePlan Until(Type phaseType, bool inclusive = true)
    {
        ArgumentNullException.ThrowIfNull(phaseType, nameof(phaseType));

        var index = Array.FindIndex(Entries, e => e.PhaseType == phaseType);
        if (index < 0)
        {
            throw new InvalidOperationException(
                $"Phase plan does not contain an entry with type '{phaseType.FullName}'."
            );
        }

        var endIndex = inclusive ? index + 1 : index;
        return new PhasePlan(Entries[..endIndex]);
    }

    /// <inheritdoc cref="Until(Type, bool)"/>
    /// <typeparam name="TPhase">The type of the phase to stop at.</typeparam>
    public PhasePlan Until<TPhase>(bool inclusive = true)
        where TPhase : Phase
    {
        return Until(typeof(TPhase), inclusive);
    }

    /// <inheritdoc/>
    public IEnumerator<Entry> GetEnumerator()
    {
        return ((IEnumerable<Entry>)Entries).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// An entry in a phase plan.
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Creates a new <see cref="Entry"/> for the given <typeparamref name="TPhase"/>.
        /// </summary>
        /// <typeparam name="TPhase">The type of the phase.</typeparam>
        /// <returns>A new <see cref="Entry"/> instance.</returns>
        public static Entry For<TPhase>()
            where TPhase : Phase
        {
            return For(typeof(TPhase));
        }

        /// <summary>
        /// Creates a new <see cref="Entry"/> for the given <paramref name="phaseType"/>.
        /// </summary>
        /// <param name="phaseType">The type of the phase.</param>
        /// <returns>A new <see cref="Entry"/> instance.</returns>
        public static Entry For(Type phaseType)
        {
            ArgumentNullException.ThrowIfNull(phaseType, nameof(phaseType));
            if (!phaseType.IsAssignableTo(typeof(Phase)))
            {
                throw new ArgumentException(
                    $"Type '{phaseType.FullName}' is not a valid Phase type.",
                    nameof(phaseType)
                );
            }

            return new Entry(phaseType);
        }

        private Entry(Type phaseType)
        {
            PhaseType = phaseType;
            PhaseName = phaseType.Name.EndsWith("Phase") ? phaseType.Name[..^"Phase".Length] : phaseType.Name;
        }

        /// <summary>
        /// The type of the phase.
        /// </summary>
        public Type PhaseType { get; }

        /// <summary>
        /// The ID of the phase.
        /// </summary>
        public string PhaseName { get; }
    }

    /// <summary>
    /// Builder used to create phase plans.
    /// </summary>
    public class Builder : IEnumerable<Entry>
    {
        /// <summary>
        /// The current entries in the phase plan, in order of execution.
        /// </summary>
        public List<Entry> Entries { get; } = [];

        /// <summary>
        /// Appends a new phase to the end of the plan, returning the builder for chaining.
        /// </summary>
        /// <typeparam name="TPhase">The type of the phase.</typeparam>
        /// <returns>The builder for chaining.</returns>
        public Builder Then<TPhase>()
            where TPhase : Phase
        {
            Entries.Add(Entry.For<TPhase>());
            return this;
        }

        /// <summary>
        /// Builds a new phase plan from the current entries in the builder.
        /// </summary>
        /// <returns>The new plan</returns>
        public PhasePlan Build()
        {
            if (Entries.Count == 0)
            {
                throw new InvalidOperationException("Cannot build a phase plan with no entries.");
            }

            return new PhasePlan(Entries);
        }

        /// <inheritdoc/>
        public IEnumerator<Entry> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
