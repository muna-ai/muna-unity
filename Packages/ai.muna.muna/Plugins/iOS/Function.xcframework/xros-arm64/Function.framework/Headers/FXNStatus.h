//
//  FXNStatus.h
//  Function
//
//  Created by Yusuf Olokoba on 10/14/2023.
//  Copyright © 2025 NatML Inc. All rights reserved.
//

#pragma once

#ifdef __cplusplus
    #define FXN_API extern "C"
#else
    #define FXN_API extern
#endif

#pragma region --Enumerations--
/*!
 @enum FXNStatus

 @abstract Function status codes.

 @constant FXN_OK
 Successful operation.

 @constant FXN_ERROR_INVALID_ARGUMENT
 Provided argument is invalid.

 @constant FXN_ERROR_INVALID_OPERATION
 Operation is invalid in current state.

 @constant FXN_ERROR_NOT_IMPLEMENTED
 Operation has not been implemented.
*/
enum FXNStatus {
    FXN_OK                       = 0,
    FXN_ERROR_INVALID_ARGUMENT   = 1,
    FXN_ERROR_INVALID_OPERATION  = 2,
    FXN_ERROR_NOT_IMPLEMENTED    = 3,
};
typedef enum FXNStatus FXNStatus;
#pragma endregion
