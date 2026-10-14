# Minimal result message - BRANCH GUIDE

Following are the most important branches:

- `develop`: Contains the latest code **and it is the branch actively developed**. Note that **all PRs must be against the `develop` branch to be considered**. This branch is developed using netstandard2.0 / netstandard2.1 (F#).
- `main`: Synced from time to time from `develop`. It contains "stable" code, although not the latest one. The plan is to do periodic merges from `develop` to `main`, but not right now.

Any other branch is considered temporary and could be deleted at any time. Do not open any PR against them!

To make new features, you branch off of `develop` into feature branches, i.e. `feature/my-new-feature`. To prep a new release for deployment, you make a release branch off of `develop`, i.e. `release/1.0.1`, which you then merge to `main`.

To do a hotfix on production/`main`, you make a hotfix branch off of `main`, i.e. `hotfix/fix-prod-bug`, which you merge back into `main`.

Thanks!
