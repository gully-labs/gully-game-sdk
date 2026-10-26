# Contributing

- Branch from `main` (`feat/...`, `fix/...`), open a PR, squash-merge. `main` is protected.
- The package has **no Unity Asset Store content and never will**: this repo is public. Do not paste code or assets
  from Gully Studio game repos here.
- Public API lives under `Runtime/`; keep it `netstandard2.1`-clean (System.Numerics is fine) and free of scene or
  MonoBehaviour dependencies. Pure helpers (`Hex`, `Keccak256`, `Abi`, `ArcadeAbi`, `Pkce`, `Base64Url`) must not
  reference `UnityEngine`.
- Every new file needs a `.meta` (`node Tools~/gen-meta.mjs`), and `node Tools~/check-package.mjs` must pass.
- Add an EditMode test for anything that encodes, decodes or signs. Run them in a Unity 6 project that references
  this package (Window > General > Test Runner).
- Bump `package.json` version + `CHANGELOG.md` in the release PR, then tag `vX.Y.Z` on `main`; consumers pin the tag.
- No secrets, keys or tokens anywhere, including tests and samples.
