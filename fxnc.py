# 
#   Muna
#   Copyright © 2026 NatML Inc. All Rights Reserved.
#

from argparse import ArgumentParser
from pathlib import Path
from requests import get
from shutil import make_archive, unpack_archive
from tempfile import TemporaryDirectory
from textwrap import dedent

parser = ArgumentParser()
parser.add_argument("--version", type=str, required=True)

def _download_fxnc(url: str, path: Path):
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

def _create_aar(
    path: str | Path,
    *,
    version: str
):
    with TemporaryDirectory() as tmpdir:
        aar_root = Path(tmpdir) / "aar"
        aar_root.mkdir()
        manifest_path = aar_root / "AndroidManifest.xml"
        manifest_path.write_text(MANIFEST_SOURCE)
        # Write armv7 lib
        armv7_url = f"https://cdn.fxn.ai/fxnc/{version}/libFunction-android-armeabi-v7a.so"
        armv7_path = aar_root / "jni" / "armeabi-v7a" / "libFunction.so"
        armv7_path.parent.mkdir(parents=True)
        armv7_path.write_bytes(get(armv7_url).content)
        # Write arm64 lib
        arm64_url = f"https://cdn.fxn.ai/fxnc/{version}/libFunction-android-arm64-v8a.so"
        arm64_path = aar_root / "jni" / "arm64-v8a" / "libFunction.so"
        arm64_path.parent.mkdir(parents=True)
        arm64_path.write_bytes(get(arm64_url).content)
        # Write metadata
        metadata_path = aar_root / "META-INF" / "com" / "android" / "build" / "gradle" / "aar-metadata.properties"
        metadata_path.parent.mkdir(parents=True)
        metadata_path.write_text(METADATA_PROPERTIES)
        # Archive
        zip_path = make_archive(
            "Muna",
            format="zip",
            root_dir=aar_root,
            base_dir="."
        )
        zip_path = Path(zip_path)
        zip_path.rename(path)
    print(f"Wrote Muna.aar to path: {path}")

def main(): # CHECK # Linux # Android AAR
    args = parser.parse_args()
    version = args.version
    # Download libs
    LIB_PATH_BASE = Path("Packages") / "ai.muna.muna" / "Plugins"
    LIBS = [
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function.xcframework.zip",
            "path": LIB_PATH_BASE / "iOS" / "Function.xcframework.zip"
        },
        {
            "url": f"https://cdn.fxn.ai/fxnc/{version}/Function-macos-arm64.dylib",
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
    # Create AAR
    aar_path = LIB_PATH_BASE / "Android" / "Muna.aar"
    _create_aar(aar_path, version=version)

MANIFEST_SOURCE = dedent("""\
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android" package="ai.muna.muna.unity">
    <application>
        <uses-native-library android:name="libOpenCL.so"       android:required="false" />
        <uses-native-library android:name="libOpenCL-pixel.so" android:required="false" />
        <uses-native-library android:name="libOpenCL-car.so"   android:required="false" />
    </application>
</manifest>
""").lstrip()

METADATA_PROPERTIES = dedent(f"""
aarFormatVersion=1.0
aarMetadataVersion=1.0
minCompileSdk=1
minCompileSdkExtension=0
minAndroidGradlePluginVersion=1.0.0
coreLibraryDesugaringEnabled=false
""").lstrip()

if __name__ == "__main__":
    main()