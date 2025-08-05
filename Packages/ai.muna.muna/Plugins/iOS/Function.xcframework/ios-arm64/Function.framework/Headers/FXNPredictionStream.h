//
//  FXNPredictionStream.h
//  Function
//
//  Created by Yusuf Olokoba on 4/19/2024.
//  Copyright © 2025 NatML Inc. All rights reserved.
//

#pragma once

#include <Function/FXNPrediction.h>

#pragma region --Types--
/*!
 @struct FXNPredictionStream
 
 @abstract Prediction stream.

 @discussion Prediction stream.
*/
struct FXNPredictionStream;
typedef struct FXNPredictionStream FXNPredictionStream;
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function FXNPredictionStreamRelease

 @abstract Release a prediction stream.

 @discussion Release a prediction stream.

 @param stream
 Prediction stream.
*/
FXN_API FXNStatus FXNPredictionStreamRelease (FXNPredictionStream* stream);
#pragma endregion


#pragma region --Operations--
/*!
 @function FXNPredictionStreamReadNext

 @abstract Read the next prediction in the stream.

 @discussion Read the next prediction in the stream.

 @param stream
 Prediction stream.

 @param prediction
 Prediction.
 You MUST release the prediction with `FXNPredictionRelease` when no longer needed.

 @returns `FXN_OK` a prediction was successfully read from the stream.
 `FXN_ERROR_INVALID_OPERATION` if the stream has no more predictions.
*/
FXN_API FXNStatus FXNPredictionStreamReadNext (
    FXNPredictionStream* stream,
    FXNPrediction** prediction
);
#pragma endregion
