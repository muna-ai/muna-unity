//
//  FXNValueList.h
//  Function
//
//  Created by Yusuf Olokoba on 12/21/2025.
//  Copyright © 2026 NatML Inc. All rights reserved.
//

#pragma once

#include <stdbool.h>
#include <Function/FXNValue.h>

#pragma region --Types--
/*!
 @struct FXNValueList
 
 @abstract Prediction value list.

 @discussion Prediction value list.
*/
typedef FXNValue FXNValueList;
#pragma endregion


#pragma region --Operations--
/*!
 @function FXNValueListGetSize

 @abstract Get the size of the value list.

 @discussion Get the size of the value list.

 @param list
 Prediction value list.

 @param size
 Output size. MUST NOT be `NULL`.
*/
FXN_API FXNStatus FXNValueListGetSize(
    FXNValueList* list,
    int32_t* size
);

/*!
 @function FXNValueListGetValue

 @abstract Get the value for a given key in the value list.

 @discussion Get the value for a given key in the value list.

 @param list
 Prediction value list.

 @param index
 Value index.

 @param value
 Output value. MUST NOT be `NULL`.
*/
FXN_API FXNStatus FXNValueListGetValue(
    FXNValueList* list,
    int32_t index,
    FXNValue** value
);

/*!
 @function FXNValueListAppendValue

 @abstract Append a value to a value list.

 @discussion Append a value to a value list.

 NOTE: The value list takes ownership of the value.
 As such, you must not call `FXNValueRelease` on the value.

 @param list
 Prediction value map.

 @param value
 Value.
*/
FXN_API FXNStatus FXNValueListAppendValue(
    FXNValueList* list,
    FXNValue* value
);
#pragma endregion
