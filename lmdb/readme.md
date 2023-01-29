### Compiling multi-platform targets for LMDB

This process has historically been very tedious and error prone due to manual nature taken in the past.
Now all targets can be handled in a single script with the exception of MacOS x64 (assuming you're on M*).
It does require a little bit of setup, but better than it used to be.

If you deal with this kind of stuff frequently and know a better way, please contribute.

This assumes you're mostly working from a Mac.

Install mingw64 build tools for targeting Windows from a Mac

`brew bundle install`

Docker has been used to deal with the various linux targets and the script assumes
you have followed the below steps prior to running it.

```bash
docker buildx build --platform linux/amd64 -t amd64-gcc . --load
docker buildx build --platform linux/arm64 -t arm64-gcc . --load
docker buildx build --platform linux/arm/7 -t arm-gcc . --load
```