### Compiling multi-platform targets for LMDB

This process has historically been very tedious and error prone due to manual nature taken in the past.
Now all targets can be handled in a single script.

If you deal with this kind of stuff frequently and know a better way, please contribute.

This assumes you're mostly working from a Mac.

Install mingw64 build tools for targeting Windows from a Mac

`brew bundle install`

Docker has been used to deal with the various linux targets and the script assumes
you have a version of docker that supports multi-platform runs with --platform.