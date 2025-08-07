## 0.0.42
+ Function is now Muna!
+ Added `PredictorAccess.Unlisted` enumeration member for public predictors excluded from discovery.

## 0.0.41
+ Added support for WebAssembly 2023 in Unity 6.1+.

## 0.0.40
+ Fixed rare `TypeLoadException: VTable setup of type T failed` exception when building from Unity.
+ Removed `fxn.Predictions.IsReady` method. You must manually track whether a predictor is loaded.
+ Upgraded to Function C 0.0.36.

## 0.0.39
+ Added `Predictor.access` field for inspecting predictor access mode.
+ Fixed linker errors due to native library being disabled when building for iOS and visionOS.
+ Refactored `AccessMode` enumeration to `PredictorAccess`.
+ Removed `Predictor.error` field.
+ Removed `PredictorStatus.Invalid` enumeration member.

## 0.0.38
+ Fixed predictor embedding failing when building for visionOS.
+ Fixed failed predictor embedding causing builds to fail.

## 0.0.37
+ Fixed compiler error when compiling for visionOS.

## 0.0.36
+ Added experimental support for Apple Vision Pro (visionOS).

## 0.0.35
+ Fixed DEX errors when building for Android on Unity 2022 LTS.
+ Refactored `PredictorStatus.Provisioning` enumeration member to `Compiling`.
+ Removed `Parameter.defaultValue` field.

## 0.0.34
+ Added `fxn.Beta.Predictions.Remote.Create` method for creating predictions that run on remote servers.
+ Removed `FunctionUnity.StreamingAssetsToAbsolutePath` method.

## 0.0.33
+ Fixed prediction embedding errors causing Unity build to fail.

## 0.0.32
+ Function can now make predictions fully offline after a predictor has been cached on-device.
+ Fixed `fxn.Predictions.Create` method failing in some release builds on Android.
+ Upgraded to Function C 0.0.34.

## 0.0.31
+ Added support for Linux `x86_64`.
+ Added `fxn.Predictions.IsReady` method to check whether a predictor is loaded and ready to make predictions.
+ Fixed `Function.dylib` macOS plugin missing when building universal macOS apps from Unity.
+ Upgraded to Function C 0.0.33.
+ Removed support for the Android `x86` platform architecture.

## 0.0.30
+ Added `FunctionAPIException` exception class for explicitly catching API errors.
+ Fixed predictions not being properly cached and requiring an internet connection on every run (#8).
+ Refactored `Acceleration.Default` enumeration member to `Acceleration.Auto`.
+ Removed `Predictor.predictions` field.
+ Upgraded to Function C 0.0.30.
+ Function now requires Unity 2022.3+

## 0.0.29
+ Fixed Android build errors caused by C++ STL in Unity 2023 and Unity 6 (#5).

## 0.0.28
+ Fixed predictions failing on Android due to predictor embedding error at build time.

## 0.0.27
+ Upgraded to Function C 0.0.27.
+ Removed `fxn.EnvironmentVariables` field.
+ Removed `fxn.Storage` field.
+ Removed `fxn.Users.Update` method.
+ Removed `fxn.Predictors.List` method.
+ Removed `fxn.Predictors.Search` method.
+ Removed `fxn.Predictors.Create` method.
+ Removed `fxn.Predictors.Delete` method.
+ Removed `fxn.Predictors.Archive` method.
+ Removed `fxn.Predictions.ToObject` method.
+ Removed `fxn.Predictions.ToValue` method.
+ Removed `Predictor.type` field.
+ Removed `Predictor.acceleration` field.
+ Removed `Prediction.type` field.
+ Removed `EnvironmentVariable` class.
+ Removed `Profile` class.
+ Removed `Tag` class.
+ Removed `Value` class.
+ Removed `UploadType` enumeration.
+ Removed `PredictorType` enumeration.
+ Removed `Acceleration.A40` enumeration member.
+ Removed `Acceleration.A100` enumeration member.

## 0.0.26
+ Fixed `WebException: The request was aborted: The request was canceled` when building for Android (#4).

## 0.0.25
+ Minor stability improvements.

## 0.0.24
+ Fixed `fxn.Predictions.Create` returning `null` for cloud predictors.
+ Fixed sporadic `InvalidOperationException` being thrown when making predictions on WebGL.

## 0.0.23
+ Fixed HTTP `403 Forbidden` errors when making some edge predictions.

## 0.0.22
+ Fixed Function API web requests failing due to internet unreachability errors on iOS (#3).
+ Fixed edge predictions being consistently recreated instead of being cached at runtime (#2).
+ Fixed WebGL build errors on Unity 2023.

## 0.0.21
+ Fixed edge prediction support on WebGL.
+ Fixed duplicate interface compiler errors when project depends on `Microsoft.Bcl.AsyncInterfaces` library.
+ Updated `fxn.Predictions.ToObject` method to return an `Image` for image values instead of a `Value`.
+ Updated `FunctionUnity.ToTexture` extension method to accept an `Image` instead of a `Value`.

## 0.0.20
+ Fixed build errors on WebGL.
+ Updated `fxn.Predictions.ToObject` method to return a `Newtonsoft.Json.Linq.JArray` insteaf of a `List<object>` for list values.
+ Updated `fxn.Predictions.ToObject` method to return a `Newtonsoft.Json.Linq.JObject` insteaf of a `Dictionary<string, object>` for dictionary values.

## 0.0.19
+ Added `PrivacyInfo.xcprivacy` iOS privacy manifest in `Function.framework`.
+ Fixed `InvalidOperationException` when edge predictions return image values.
+ Fixed Apple App Store upload errors due to incorrect `CFBundleVersion` key in `Function.framework`.
+ Fixed embedded edge predictors failing to load from cache causing unnecessary downloads.
+ Fixed Android build errors when embedding edge predictors.
+ Updated to Function C 0.0.23.

## 0.0.18
+ Fixed edge predictions failing when passing in `Dictionary<TKey, TValue>` input values.
+ Updated to Function C 0.0.20.

## 0.0.17
+ Fixed edge prediction errors caused by request backpressure while the predictor is being loaded.

## 0.0.16
+ Added support for `Enum` input values in `Function.Predictions.Create` method.
+ Fixed JSON deserialization errors caused by code stripping in builds.
+ Removed `Function.Types.Converters` namespace. Bring your own JSON converters.

## 0.0.15
+ Minor stability improvements.

## 0.0.14
+ Fixed Apple App Store app rejections due to missing Bundle Version key in Function.framework.
+ Updated to Function C 0.0.18.

## 0.0.13
+ Added support for Unity 2021 LTS.
+ Updated `FunctionUnity.ToImage` extension method to accept an optional buffer to avoid allocating memory.
+ Removed `FunctionUnity.ToValue(Texture2D)` extension method. Use `FunctionUnity.ToImage` method instead.

## 0.0.12
+ Added `Function.Types.Image` struct for making edge predictions on images.
+ Added `FunctionUnity.ToImage(Texture2D)` helper function for creating an image from a `Texture2D`.

## 0.0.11
+ Fixed WebGL build error when building in Release mode due to JavaScript minification.

## 0.0.10
+ Fixed WebGL build error when Function is installed with Unity Package Manager.

## 0.0.9
+ Added support for making on-device predictions on Android.
+ Added support for making on-device predictions on iOS.
+ Added support for making on-device predictions on macOS.
+ Added support for making on-device predictions on WebGL.
+ Added support for making on-device predictions on Windows.

## 0.0.8
+ Fixed linker errors when compiling iOS projects in Xcode.

## 0.0.7
+ Fixed compiler error on some platforms related to Function version.

## 0.0.6
+ Fixed `NullReferenceException` when calling `Tag.TryParse` with `null` input string.
+ Removed `CloudPrediction` class. Use `Prediction` class instead.
+ Removed `EdgePrediction` class. Use `Prediction` class instead.

## 0.0.5
+ Added `Function.Predictions.Stream` method for making streaming predictions.
+ Refactored `IGraphClient` interface to `IFunctionClient`.
+ Refactored `UnityGraphClient` class to `UnityClient`.
+ Refactored `Function.Graph` namespace to `Function.API`.

## 0.0.4
+ Minor updates.

## 0.0.3
+ Refactored `Predictor.readme` field to `card`.
+ Refactored `Dtype.Undefined` enumeration member to `Dtype.Null`.

## 0.0.2
+ Added `FunctionUnity.ToAudioClip` extension method for converting a Function `Value` to an `AudioClip`.
+ Fixed Function access key not being available in builds.
+ Refactored `Feature` class to `Value` for improved clarity.
+ Refactored `Function.Predictions.ToFeature` method to `Function.Predictions.ToValue`.
+ Refactored `Function.Predictions.ToValue` method to `Function.Predictions.ToObject`.
+ Refactored `FunctionUnity.ToFeature` extension method to `FunctionUnity.ToValue`.
+ Refactored `UploadType.Feature` enumeration member to `UploadType.Value`.

## 0.0.1
+ First pre-release.