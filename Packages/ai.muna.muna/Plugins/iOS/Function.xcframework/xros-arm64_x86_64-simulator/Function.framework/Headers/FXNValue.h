//
//  FXNValue.h
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
 @enum FXNDtype

 @abstract Value data type.

 @constant FXN_DTYPE_NULL
 Null or undefined.

 @constant FXN_DTYPE_FLOAT16
 IEEE 754 half precision 16-bit float.

 @constant FXN_DTYPE_FLOAT32
 IEEE 754 single precision 32-bit float.

 @constant FXN_DTYPE_FLOAT64
 IEEE 754 double precision 64-bit float.

 @constant FXN_DTYPE_INT8
 Signed 8-bit integer.

 @constant FXN_DTYPE_INT16
 Signed 16-bit integer.

 @constant FXN_DTYPE_INT32
 Signed 32-bit integer.

 @constant FXN_DTYPE_INT64
 Signed 64-bit integer.

 @constant FXN_DTYPE_UINT8
 Unsigned 8-bit integer.

 @constant FXN_DTYPE_UINT16
 Unsigned 16-bit integer.

 @constant FXN_DTYPE_UINT32
 Unsigned 32-bit integer.

 @constant FXN_DTYPE_UINT64
 Unsigned 64-bit integer.

 @constant FXN_DTYPE_BOOL
 8-bit boolean where zero is `false` and non-zero is `true`.

 @constant FXN_DTYPE_STRING
 UTF-8 encoded string.

 @constant FXN_DTYPE_LIST
 JSON-serializable list.

 @constant FXN_DTYPE_DICT
 JSON-serializable dictionary.

 @constant FXN_DTYPE_IMAGE
 Pixel buffer with 8 bits per intensity, interleaved by channel.

 @constant FXN_DTYPE_BINARY
 Binary blob.

 @constant FXN_DTYPE_BFLOAT16
 16-bit brain float.
*/
enum FXNDtype {
    FXN_DTYPE_NULL      = 0,
    FXN_DTYPE_FLOAT16   = 1,
    FXN_DTYPE_FLOAT32   = 2,
    FXN_DTYPE_FLOAT64   = 3,
    FXN_DTYPE_INT8      = 4,
    FXN_DTYPE_INT16     = 5,
    FXN_DTYPE_INT32     = 6,
    FXN_DTYPE_INT64     = 7,
    FXN_DTYPE_UINT8     = 8,
    FXN_DTYPE_UINT16    = 9,
    FXN_DTYPE_UINT32    = 10,
    FXN_DTYPE_UINT64    = 11,
    FXN_DTYPE_BOOL      = 12,
    FXN_DTYPE_STRING    = 13,
    FXN_DTYPE_LIST      = 14,
    FXN_DTYPE_DICT      = 15,
    FXN_DTYPE_IMAGE     = 16,
    FXN_DTYPE_BINARY    = 17,
    FXN_DTYPE_BFLOAT16  = 18,
};
typedef enum FXNDtype FXNDtype;

/*!
 @enum FXNValueFlags

 @abstract Value flags.

 @constant FXN_VALUE_FLAG_NONE
 No flags.

 @constant FXN_VALUE_FLAG_COPY_DATA
 Copy input data when creating the value.
 When this flag is not set, the value data MUST remain valid for the lifetime of the created value.
*/
enum FXNValueFlags {
    FXN_VALUE_FLAG_NONE         = 0,
    FXN_VALUE_FLAG_COPY_DATA    = 1,
};
typedef enum FXNValueFlags FXNValueFlags;
#pragma endregion


#pragma region --Types--
/*!
 @struct FXNValue
 
 @abstract Prediction input or output value.

 @discussion Prediction input or output value.
*/
struct FXNValue;
typedef struct FXNValue FXNValue;
#pragma endregion


#pragma region --Lifecycle--
/*!
 @function FXNValueRelease

 @abstract Release a value.

 @discussion Release a value.

 @param value
 Value.
*/
FXN_API FXNStatus FXNValueRelease (FXNValue* value);
#pragma endregion


#pragma region --Operations--
/*!
 @function FXNValueGetData

 @abstract Get the value data.

 @discussion Get the value data.

 @param value
 Value.

 @param data
 Opaque pointer to value data.
*/
FXN_API FXNStatus FXNValueGetData (
    FXNValue* value,
    void** data
);

/*!
 @function FXNValueGetType

 @abstract Get the data type of a given value.

 @discussion Get the data type of a given valuie.

 @param value
 Value.

 @param type
 Value data type.
*/
FXN_API FXNStatus FXNValueGetType (
    FXNValue* value,
    FXNDtype* type
);

/*!
 @function FXNValueGetDimensions

 @abstract Get the number of dimensions for a given value.

 @discussion Get the number of dimensions for a given value.
 If the type is not a tensor, this function will return zero.

 @param value
 Value.

 @param dimensions
 Number of dimensions for a given value.
*/
FXN_API FXNStatus FXNValueGetDimensions (
    FXNValue* value,
    int32_t* dimensions
);

/*!
 @function FXNValueGetShape

 @abstract Get the shape of a given value.

 @discussion Get the shape of a given value.

 @param value
 Value.

 @param shape
 Destination shape array.

 @param shapeLen
 Length of the destination array in elements.
*/
FXN_API FXNStatus FXNValueGetShape (
    FXNValue* value,
    int32_t* shape,
    int32_t shapeLen
);
#pragma endregion


#pragma region --Constructors--
/*!
 @function FXNValueCreateArray

 @abstract Create an array value from a data buffer.

 @discussion Create an array value from a data buffer.

 @param data
 Array data.
 Can be `NULL` in which case the value will allocate its own memory.

 @param shape
 Array shape.
 Can be `NULL` for scalar values.

 @param dims
 Number of dimensions in `shape`.
 Zero dims indicates a scalar value.

 @param dtype
 Value data type.

 @param flags
 Value creation flags.

 @param value
 Created value.
*/
FXN_API FXNStatus FXNValueCreateArray (
    void* data,
    const int32_t* shape,
    int32_t dims,
    FXNDtype dtype,
    FXNValueFlags flags,
    FXNValue** value
);

/*!
 @function FXNValueCreateString

 @abstract Create a string value from a UTF-8 encoded string.
 
 @discussion Create a string value from a UTF-8 encoded string.

 @param data
 UTF-8 encoded string.

 @param value
 Created value.
*/
FXN_API FXNStatus FXNValueCreateString (
    const char* data,
    FXNValue** value
);

/*!
 @function FXNValueCreateList

 @abstract Create a list value from a JSON-encoded list.
 
 @discussion Create a list value from a JSON-encoded list.

 @param data
 JSON-encoded list.

 @param value
 Created value.
*/
FXN_API FXNStatus FXNValueCreateList (
    const char* data,
    FXNValue** value
);

/*!
 @function FXNValueCreateDict

 @abstract Create a dictionary value from a JSON-encoded dictionary.
 
 @discussion Create a dictionary value from a JSON-encoded dictionary.

 @param data
 JSON-encoded dictionary.

 @param value
 Created value.
*/
FXN_API FXNStatus FXNValueCreateDict (
    const char* data,
    FXNValue** value
);

/*!
 @function FXNValueCreateImage

 @abstract Create an image value from a pixel buffer.

 @discussion Create an image value from a pixel buffer.
 The pixel buffer MUST have an interleaved R8 (8bpp), RGB888 (24bpp), or RGBA8888 layout (32bpp).

 @param pixelBuffer
 Pixel buffer.

 @param width
 Pixel buffer width.

 @param height
 Pixel buffer height.

 @param channels
 Pixel buffer channels.
 MUST be 1, 3, or 4.

 @param flags
 Value creation flags.

 @param value
 Created value.
 The value `type` will be `FXN_DTYPE_IMAGE`.
 The value `shape` will be `(H,W,C)`.
*/
FXN_API FXNStatus FXNValueCreateImage (
    const uint8_t* pixelBuffer,
    int32_t width,
    int32_t height,
    int32_t channels,
    FXNValueFlags flags,
    FXNValue** value
);

/*!
 @function FXNValueCreateBinary

 @abstract Create a binary value from a raw buffer.
 
 @discussion Create a binary value from a raw buffer.

 @param buffer
 Buffer.

 @param bufferLen
 Buffer length in bytes.

 @param flags
 Value creation flags.

 @param value
 Created value.
*/
FXN_API FXNStatus FXNValueCreateBinary (
    void* buffer,
    int32_t bufferLen, // CHECK // max buffer size becomes 2GB
    FXNValueFlags flags,
    FXNValue** value
);

/*!
 @function FXNValueCreateNull

 @abstract Create a null value.
 
 @discussion Create a null value.

 @param value
 Created value.
*/
FXN_API FXNStatus FXNValueCreateNull (FXNValue** value);
#pragma endregion
