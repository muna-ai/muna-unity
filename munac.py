# 
#   Function
#   Copyright © 2025 NatML Inc. All Rights Reserved.
#

from argparse import ArgumentParser
from pathlib import Path
from requests import get
from shutil import unpack_archive

parser = ArgumentParser()
parser.add_argument("--version", type=str, required=True)

def _download_fxnc (url: str, path: Path):
    # Download
    response = get(url)
    response.raise_for_status()
    with open(path, "wb") as f:
        f.write(response.content)
    print(f"Wrote {url} to path: {path}")
    # Unzip
    if path.suffix == ".zip":
        unpack_archive(path, extract_dir=path.parent)
        path.unlink()
        print(f"Extracted {path}")

def main (): # CHECK # Linux # Android AAR
    args = parser.parse_args()
    version = args.version
    LIB_PATH_BASE = Path("Packages") / "ai.fxn.fxn3d" / "Plugins"
    LIBS = [
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function.xcframework.zip",
            "path": LIB_PATH_BASE / "iOS" / "Function.xcframework.zip"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function-macos.dylib",
            "path": LIB_PATH_BASE / "macOS" / "Function.dylib"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/libFunction-linux-arm64.so",
            "path": LIB_PATH_BASE / "Linux" / "arm64" / "libFunction.so"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/libFunction-linux-x86_64.so",
            "path": LIB_PATH_BASE / "Linux" / "x86_64" / "libFunction.so"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function-win-x86_64.dll",
            "path": LIB_PATH_BASE / "Windows" / "x86_64" / "Function.dll"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function-win-arm64.dll",
            "path": LIB_PATH_BASE / "Windows" / "arm64" / "Function.dll"
        },
    ]
    for lib in LIBS:
        _download_fxnc(lib["url"], lib["path"])

if __name__ == "__main__":
    main()