# Open-RSC/Core-Framework Reference Plan

The Open-RSC/Core-Framework repository could not be downloaded from GitHub in this environment. The clone attempt failed with a `CONNECT tunnel failed, response 403` error, indicating outbound network access to GitHub is blocked here.

## Current status
- No offline archive for Open-RSC/Core-Framework is present under `docs/reference/` after the latest sync of this repository.
- The local Git checkout does not have a configured `origin`, so there is no remote to pull a pre-packaged archive from.
- Direct cloning from GitHub is still blocked in this environment (latest check reproduced the 403).

## Pending archive
To keep a reference snapshot of the upstream codebase inside this project, download the repository when network access is available and add a compressed archive under `docs/reference/` (for example, `Open-RSC-Core-Framework.zip`). Suggested steps:

1. Clone or download the repository locally:
   ```bash
   git clone --depth 1 https://github.com/Open-RSC/Core-Framework
   ```
2. Package the working tree (excluding the `.git` directory) into a zip file:
   ```bash
   cd Core-Framework
   zip -r ../Open-RSC-Core-Framework.zip . -x '.git/*'
   ```
3. Place the zip file in this project's `docs/reference/` directory and commit it for offline reference.

## Follow-up review
Once an archive is available, we can review the upstream source to identify deltas and prioritize changes for this codebase. Please provide the archive or enable network access so a comparison can be performed.
