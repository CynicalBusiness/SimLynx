# Loader Implementation Plan

Status: provisional. Loader-facing work should follow stabilization of the code-first prototype compilation semantics.
This document captures the intended feature set and open questions, not a finalized implementation contract.

## Intent

Loaders should make prototype configuration available to files, packages, and UGC while retaining deterministic
composition, useful source-aware diagnostics, and the same validated compilation path as the code-first API.

## Design Gaps

The current code-first model does not yet provide:

- source metadata such as package, loader, file, and line;
- ordered patches from multiple contributors;
- deferred base-ID resolution and forward references;
- conflict diagnostics and invalid override reporting;
- file-based loaders.

## Initial Implementation Steps

1. Define source-aware design data and patch provenance.
2. Define deterministic patch ordering across files and packages.
3. Add deferred ID resolution, including forward references.
4. Add conflict and invalid-override diagnostics suitable for content authors.
5. Define supported file formats and implement their loaders.
6. Route loaded definitions through prototype graph validation and compilation.
7. Define the security boundary for untrusted UGC, including which properties and configuration surfaces loaders may
   access.

## Open Questions

- Should `Base` remain a direct reference in the code-first API while loaders use deferred IDs, or should all design
  prototypes store IDs until validation?
- Should property configurability remain public-setter-by-default when untrusted UGC loaders are introduced?
- How should patches from multiple packages be ordered, diagnosed, and overridden?
- Which source metadata must survive into compiled blueprints for inspection and runtime diagnostics?
- Should loaders produce prototype mutations directly or an intermediate patch representation?
