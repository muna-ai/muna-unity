/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

#pragma once

/**
 @abstract Function dialect definition.
 
 @discussion This header allows for bridging Python operators to equivalent implementations in C/C++.
 
 NOTE: This header is EXPERIMENTAL.
*/

#pragma region --Platform--
/*!
 @abstract Unknown platform.
*/
#define FXN_PLATFORM_UNKNOWN        0

/*!
 @abstract Android armeabi-v7a platform.
*/
#define FXN_PLATFORM_ANDROID_ARM    (1 << 0)

/*!
 @abstract Android arm64-v8a platform.
*/
#define FXN_PLATFORM_ANDROID_ARM64  (1 << 1)

/*!
 @abstract Android x86 platform.
 @deprecated
*/
#define FXN_PLATFORM_ANDROID_X86    (1 << 2)

/*!
 @abstract Android x86_64 platform.
 @deprecated
*/
#define FXN_PLATFORM_ANDROID_X64    (1 << 3)

/*!
 @abstract Android platform across all architectures.
*/
#define FXN_PLATFORM_ANDROID        (FXN_PLATFORM_ANDROID_ARM | FXN_PLATFORM_ANDROID_ARM64)

/*!
 @abstract iOS arm64 platform.
*/
#define FXN_PLATFORM_IOS_ARM64      (1 << 4)

/*!
 @abstract iOS platform across all architectures.
*/
#define FXN_PLATFORM_IOS            FXN_PLATFORM_IOS_ARM64

/*!
 @abstract macOS x86_64 platform.
 @deprecated
*/
#define FXN_PLATFORM_MACOS_X64      (1 << 5)

/*!
 @abstract macOS arm64 platform.
*/
#define FXN_PLATFORM_MACOS_ARM64    (1 << 6)

/*!
 @abstract macOS platform across all architectures.
*/
#define FXN_PLATFORM_MACOS          FXN_PLATFORM_MACOS_ARM64

/*!
 @abstract Linux x86_64 platform.
*/
#define FXN_PLATFORM_LINUX_X64      (1 << 7)

/*!
 @abstract Linux arm64 platform.
*/
#define FXN_PLATFORM_LINUX_ARM64    (1 << 8)

/*!
 @abstract Linux platform across all architectures.
*/
#define FXN_PLATFORM_LINUX          (FXN_PLATFORM_LINUX_X64 | FXN_PLATFORM_LINUX_ARM64)

/*!
 @abstract visionOS arm64 platform.
*/
#define FXN_PLATFORM_VISIONOS_ARM64 (1 << 13)

/*!
 @abstract visionOS platform across all architectures.
*/
#define FXN_PLATFORM_VISIONOS       FXN_PLATFORM_VISIONOS_ARM64

/*!
 @abstract WebAssembly 32-bit platform.
*/
#define FXN_PLATFORM_WASM32         (1 << 9)

/*!
 @abstract WebAssembly 64-bit platform (MEMORY64).
*/
#define FXN_PLATFORM_WASM64         (1 << 12)

/*!
 @abstract WebAssembly platform across all architectures.
*/
#define FXN_PLATFORM_WASM           FXN_PLATFORM_WASM32

/*!
 @abstract Windows x86_64 platform.
*/
#define FXN_PLATFORM_WINDOWS_X64    (1 << 10)

/*!
 @abstract Windows arm64 platform.
*/
#define FXN_PLATFORM_WINDOWS_ARM64  (1 << 11)

/*!
 @abstract Windows platform across all architectures.
*/
#define FXN_PLATFORM_WINDOWS        (FXN_PLATFORM_WINDOWS_X64 | FXN_PLATFORM_WINDOWS_ARM64)
#pragma endregion


#pragma region --Operators--
/*!
 @abstract Function operator.

 @discussion The enclosing class or struct defines a function operator.

 @param id
 Operator identifier.
*/
#define FXN_OP(id)

/*!
 @abstract Fully-qualified Python function names.

 @discussion Fully-qualified names of the Python functions that this operator implements.
 The first name is the public name of the function (i.e. how the function is referenced by users in Python).
 Subsequent names are internal aliases, used to resolve functions that are re-exported (e.g. implemented natively).

 @param qualname
 Fully-qualified function name.
*/
#define FXN_OP_FUNC(qualname, ...)

/*!
 @abstract Fully-qualified Python method names.

 @discussion Fully-qualified names of the Python methods that this operator implements.
 The first name is the public name of the method (i.e. how the method is referenced by users in Python).
 Subsequent names are internal aliases, used to resolve methods that are re-exported (e.g. implemented natively).

 @param qualname
 Fully-qualified function name.
*/
#define FXN_OP_METHOD(qualname, ...)

/*!
 @abstract Operator desscription.

 @discussion The description is useful for providing analytics and diagnostic information.
 This macro is required.

 @param description
 Operator description.
*/
#define FXN_OP_DESCRIPTION(description)

/*!
 @abstract Operator supported platforms.

 @discussion Specify platform where the operator is supported.
 This macro is required.

 @param platform
 Supported platform(s). Use `|` to specify multiple platforms.
*/
#define FXN_OP_PLATFORM(platform)

/*!
 @abstract Operator library dependency.

 @discussion Specify a library dependency for the operator.
 An operator can define multiple library dependencies.

 @param target
 Target to link against when compiling the predictor.
 This is usually the name of a library or framework.

 @param platform
 Specify the platform that this library dependency applies to.

 @param include
 CMake include definition for defining the `target`.
 Pass `FXN_LIBRARY_NO_INCLUDE` if there is no include.

 @param order
 Library linking order. Defaults to 0.
 
 @see FXNPlatform
 @see FXN_LIBRARY_NO_INCLUDE
*/
#define FXN_OP_LIBRARY(target, platform, include, ...)

/*!
 @abstract Operator metadata.

 @discussion Specify operator metadata as a key-value pair.
 This is useful in analytics and telemetry.
 
 @param key
 Metadata key.

 @param value
 Metadata value.
*/
#define FXN_OP_METADATA(key, value)

/*!
 @abstract Operator documentation URL.

 @discussion Specify the operator documentation URL.
 
 @param url
 Operator documentation URL.
*/
#define FXN_OP_DOCS(url)

/*!
 @abstract Operator method argument.

 @discussion Specify an operator argument that binds to a Python positional argument.

 @param description
 Argument description.
*/
#define FXN_OP_ARG(description)

/*!
 @abstract Operator method keyword argument.

 @discussion Specify an operator argument that binds to a Python keyword argument.

 @param kw
 Python argument keyword.

 @param description
 Argument description.
*/
#define FXN_OP_KWARG(kw, ...)

/*!
 @abstract Operator method return is iterable.

 @discussion Specify that an operator return type is iterable.

 @param element_type
 Iterable element type.
*/
#define FXN_OP_ITERABLE(element_type, ...)

/*!
 @abstract Operator is stateless.

 @discussion Specify that an operator is stateless.
 This enables us to optimize its usage to use no space.
*/
#define FXN_OP_STATELESS()
#pragma endregion


#pragma region --Operator Constants--
/*!
 @abstract Operator library has no CMake include.

 @discussion Use this to specify when an operator library has no CMake include file.
 This is usually the case when linking against system libraries.

 @see FXN_OP_LIBRARY
*/
#define FXN_LIBRARY_NO_INCLUDE ""
#pragma endregion


#pragma region --Compiler Defines--
#ifndef FXN_OP_PARSING
/*!
 @abstract Definition that resolves to 1 when the compiler is being used to parse C++ operators.

 @discussion Definition that resolves to 1 when the compiler is being used to parse C++ operators.
 This can be used to define proxy types that to assist in parsing nested template types.
*/
#define FXN_OP_PARSING 0
#endif

#ifndef FXN_CONFIGURATION
/*!
 @abstract Reference to the predictor's runtime configuration.

 @discussion Reference to the predictor's runtime configuration.
 Dialect authors should reference `FXN_CONFIGURATION` and never the
 default expansion below; the default is an implementation detail that
 may change.
*/
#define FXN_CONFIGURATION __muna_configuration
#endif
#pragma endregion