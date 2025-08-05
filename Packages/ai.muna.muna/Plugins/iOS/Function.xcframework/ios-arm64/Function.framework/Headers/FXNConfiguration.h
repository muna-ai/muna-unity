//
//  FXNConfiguration.h
//  Function
//
//  Created by Yusuf Olokoba on 5/27/2023.
//  Copyright © 2025 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <Function/FXNStatus.h>

#pragma region --Enumerations--
/*!
 @enum FXNAcceleration

 @abstract Hardware acceleration used for predictions.

 @constant FXN_ACCELERATION_AUTO
 Use the optimal acceleration for the current device.

 @constant FXN_ACCELERATION_CPU
 Use the CPU.
 
 @constant FXN_ACCELERATION_GPU
 Use the GPU.
 
 @constant FXN_ACCELERATION_NPU
 Use the neural processing unit.
*/
enum FXNAcceleration {
    FXN_ACCELERATION_AUTO   = 0,
    FXN_ACCELERATION_CPU    = 1 << 0,
    FXN_ACCELERATION_GPU    = 1 << 1,
    FXN_ACCELERATION_NPU    = 1 << 2,
};
typedef enum FXNAcceleration FXNAcceleration;
#pragma endregion


#pragma region --Types--
/*!
 @struct FXNConfiguration

 @abstract Opaque type for predictor configuration.
*/
struct FXNConfiguration;
typedef struct FXNConfiguration FXNConfiguration;

/*!
 @typedef FXNConfigurationAddResourceHandler

 @abstract Callback invoked when a configuration resource has been loaded.
 
 @param context
 User context.

 @param status
 Result status.
*/
typedef void (*FXNConfigurationAddResourceHandler) (
    void* context,
    FXNStatus status
);
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function FXNConfigurationGetUniqueID

 @abstract Get the configuration unique identifier.

 @discussion Get the configuration unique identifier.

 @param identifier
 Configuration unique identifier.

 @param size
 Identifier size.
*/
FXN_API FXNStatus FXNConfigurationGetUniqueID (
    char* identifier,
    int32_t size
);

/*!
 @function FXNConfigurationGetClientID

 @abstract Get the configuration client identifier.

 @discussion Get the configuration client identifier.

 @param identifier
 Configuration client identifier.

 @param size
 Identifier size.
*/
FXN_API FXNStatus FXNConfigurationGetClientID (
    char* identifier,
    int32_t size
);

/*!
 @function FXNConfigurationCreate

 @abstract Create a predictor configuration.

 @discussion Create a predictor configuration.

 @param configuration
 Created configuration. MUST NOT be `NULL`.
*/
FXN_API FXNStatus FXNConfigurationCreate (FXNConfiguration** configuration);

/*!
 @function FXNConfigurationRelease

 @abstract Release the predictor configuration.

 @discussion Release the predictor configuration.

 @param configuration
 Predictor configuration.
*/
FXN_API FXNStatus FXNConfigurationRelease (FXNConfiguration* configuration);
#pragma endregion


#pragma region --Configuration--
/*!
 @function FXNConfigurationGetTag

 @abstract Get the predictor tag.

 @discussion Get the predictor tag.

 @param configuration
 Predictor configuration.

 @param tag
 Destination buffer.

 @param size
 Size of destination buffer.
*/
FXN_API FXNStatus FXNConfigurationGetTag (
    FXNConfiguration* configuration,
    char* tag,
    int32_t size
);

/*!
 @function FXNConfigurationSetTag

 @abstract Set the predictor tag.

 @discussion Set the predictor tag.
 This is requried for Function to load the predictor.

 @param configuration
 Predictor configuration.

 @param tag
 Predictor tag.
*/
FXN_API FXNStatus FXNConfigurationSetTag (
    FXNConfiguration* configuration,
    const char* tag
);

/*!
 @function FXNConfigurationGetToken

 @abstract Get the configuration token.

 @discussion Get the configuration token.

 @param configuration
 Predictor configuration.

 @param token
 Destination buffer.

 @param size
 Size of destination buffer.
*/
FXN_API FXNStatus FXNConfigurationGetToken (
    FXNConfiguration* configuration,
    char* token,
    int32_t size
);

/*!
 @function FXNConfigurationSetToken

 @abstract Set the configuration token.

 @discussion Set the configuration token.
 This is requried for Function to load the predictor.

 @param configuration
 Predictor configuration.

 @param token
 Configuration token.
*/
FXN_API FXNStatus FXNConfigurationSetToken (
    FXNConfiguration* configuration,
    const char* token
);

/*!
 @function FXNConfigurationGetAcceleration

 @abstract Get the acceleration used for making predictions.

 @discussion Get the acceleration used for making predictions.

 @param configuration
 Predictor configuration.

 @param acceleration
 Acceleration.
*/
FXN_API FXNStatus FXNConfigurationGetAcceleration (
    FXNConfiguration* configuration,
    FXNAcceleration* acceleration
);

/*!
 @function FXNConfigurationSetAcceleration

 @abstract Specify the acceleration used for making predictions.

 @discussion Specify the acceleration used for making predictions.

 @param configuration
 Predictor configuration.

 @param acceleration
 Acceleration used for making predictions predictions.
*/
FXN_API FXNStatus FXNConfigurationSetAcceleration (
    FXNConfiguration* configuration,
    FXNAcceleration acceleration
);

/*!
 @function FXNConfigurationGetDevice

 @abstract Get the compute device used for compute acceleration.

 @discussion Get the compute device used for compute acceleration.

 @param configuration
 Predictor configuration.

 @param device
 Compute device.
 The type of this device is platform-dependent.
 See https://docs.fxn.ai/workflows/realtime#specifying-the-acceleration-device for more information.
*/
FXN_API FXNStatus FXNConfigurationGetDevice (
    FXNConfiguration* configuration,
    void** device
);

/*!
 @function FXNConfigurationSetDevice

 @abstract Specify the compute device used for compute acceleration.

 @discussion Specify the compute device used for compute acceleration.

 @param configuration
 Predictor configuration.

 @param device
 Compute device.
 The type of this device is platform-dependent.
 Pass `NULL` to use the default compute device.
 See https://docs.fxn.ai/workflows/realtime#specifying-the-acceleration-device for more information.
*/
FXN_API FXNStatus FXNConfigurationSetDevice (
    FXNConfiguration* configuration,
    void* device
);

/*!
 @function FXNConfigurationAddResource

 @abstract Add a prediction resource.

 @discussion Add a prediction resource.

 @param configuration
 Predictor configuration.

 @param type
 Resource type.

 @param path
 Resource path.
*/
FXN_API FXNStatus FXNConfigurationAddResource (
    FXNConfiguration* configuration,
    const char* type,
    const char* path
);
#pragma endregion
