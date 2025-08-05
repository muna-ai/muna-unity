//
//  FXNPrediction.h
//  Function
//
//  Created by Yusuf Olokoba on 11/23/2023.
//  Copyright © 2025 NatML Inc. All rights reserved.
//

#pragma once

#include <stdint.h>
#include <Function/FXNValueMap.h>

#pragma region --Types--
/*!
 @struct FXNPrediction
 
 @abstract Prediction.

 @discussion Prediction.
*/
struct FXNPrediction;
typedef struct FXNPrediction FXNPrediction;
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function FXNPredictionRelease

 @abstract Release a prediction.

 @discussion Release a prediction.

 @param prediction
 Prediction.
*/
FXN_API FXNStatus FXNPredictionRelease (FXNPrediction* prediction);
#pragma endregion


#pragma region --Operations--
/*!
 @function FXNPredictionGetID

 @abstract Get the prediction ID.

 @discussion Get the prediction ID.

 @param prediction
 Prediction.

 @param destination
 Destination buffer.

 @param size
 Destination buffer size.
*/
FXN_API FXNStatus FXNPredictionGetID (
    FXNPrediction* prediction,
    char* destination,
    int32_t size
);

/*!
 @function FXNPredictionGetLatency

 @abstract Get the prediction latency.

 @discussion Get the prediction latency.

 @param prediction
 Prediction.

 @param latency
 Prediction latency in milliseconds.
*/
FXN_API FXNStatus FXNPredictionGetLatency (
    FXNPrediction* prediction,
    double* latency
);

/*!
 @function FXNPredictionGetResults

 @abstract Get the prediction results.

 @discussion Get the prediction results.

 @param prediction
 Prediction.

 @param map
 Prediction output value map. Do NOT release this value map as it is owned by the prediction.
*/
FXN_API FXNStatus FXNPredictionGetResults (
    FXNPrediction* prediction,
    FXNValueMap** map
);

/*!
 @function FXNPredictionGetError

 @abstract Get the prediction error.

 @discussion Get the prediction error.

 @param prediction
 Prediction.

 @param error
 Destination buffer.

 @param size
 Destination buffer size.

 @returns `FXN_OK` if an error has been copied.
 `FXN_ERROR_INVALID_OPERATION` if no error exists.
*/
FXN_API FXNStatus FXNPredictionGetError (
    FXNPrediction* prediction,
    char* error,
    int32_t size
);

/*!
 @function FXNPredictionGetLogs

 @abstract Get the prediction logs.

 @discussion Get the prediction logs.

 @param prediction
 Prediction.

 @param logs
 Destination buffer.

 @param size
 Destination buffer size.
*/
FXN_API FXNStatus FXNPredictionGetLogs (
    FXNPrediction* prediction,
    char* logs,
    int32_t size
);

/*!
 @function FXNPredictionGetLogLength

 @abstract Get the prediction log length.

 @discussion Get the prediction log length.

 @param prediction
 Prediction.

 @param length
 Logs length.
*/
FXN_API FXNStatus FXNPredictionGetLogLength (
    FXNPrediction* prediction,
    int32_t* length
);
#pragma endregion
