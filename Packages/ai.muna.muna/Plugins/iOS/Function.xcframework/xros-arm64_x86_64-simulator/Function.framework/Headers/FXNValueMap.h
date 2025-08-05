//
//  FXNValueMap.h
//  Function
//
//  Created by Yusuf Olokoba on 10/14/2023.
//  Copyright © 2025 NatML Inc. All rights reserved.
//

#pragma once

#include <stdbool.h>
#include <Function/FXNValue.h>

#pragma region --Types--
/*!
 @struct FXNValueMap
 
 @abstract Predictor value map.

 @discussion Predictor value map.
*/
struct FXNValueMap;
typedef struct FXNValueMap FXNValueMap;
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function FXNValueMapCreate

 @abstract Create a prediction value map.

 @discussion Create a prediction value map.

 @param map
 Created value map. MUST NOT be `NULL`.
*/
FXN_API FXNStatus FXNValueMapCreate (FXNValueMap** map);

/*!
 @function FXNValueMapRelease

 @abstract Release the prediction value map.

 @discussion Release the prediction value map.
 This releases all values currently within the map.

 @param map
 Prediction value map.
*/
FXN_API FXNStatus FXNValueMapRelease (FXNValueMap* map);
#pragma endregion


#pragma region --Operations--
/*!
 @function FXNValueMapGetSize

 @abstract Get the size of the value map.

 @discussion Get the size of the value map.

 @param map
 Prediction value map.

 @param size
 Output size. MUST NOT be `NULL`.
*/
FXN_API FXNStatus FXNValueMapGetSize (
    FXNValueMap* map,
    int32_t* size
);

/*!
 @function FXNValueMapGetKey

 @abstract Get the key at a given index in the value map.

 @discussion Get the key at a given index in the value map.

 @param map
 Prediction value map.

 @param index
 Key index. Must be less than the value map size.

 @param key
 Destination UTF-8 encoded key string.

 @param size
 Size of destination buffer.
*/
FXN_API FXNStatus FXNValueMapGetKey (
    FXNValueMap* map,
    int32_t index,
    char* key,
    int32_t size
);

/*!
 @function FXNValueMapGetValue

 @abstract Get the value for a given key in the value map.

 @discussion Get the value for a given key in the value map.

 @param map
 Prediction value map.

 @param key
 Value key.

 @param value
 Output value. MUST NOT be `NULL`.

 @returns `FXN_OK` if the value map contains a value for the given key else `FXN_ERROR_INVALID_ARGUMENT`.
*/
FXN_API FXNStatus FXNValueMapGetValue (
    FXNValueMap* map,
    const char* key,
    FXNValue** value
);

/*!
 @function FXNValueMapSetValue

 @abstract Set the value for a given key in the value map.

 @discussion Set the value for a given key in the value map.

 NOTE: The value map takes ownership of the value.
 As such, you must not call `FXNValueRelease` on the value.

 @param map
 Prediction value map.

 @param key
 Value key.

 @param value
 Value. Pass `NULL` to remove the value from the map if present.
*/
FXN_API FXNStatus FXNValueMapSetValue (
    FXNValueMap* map,
    const char* key,
    FXNValue* value
);
#pragma endregion
