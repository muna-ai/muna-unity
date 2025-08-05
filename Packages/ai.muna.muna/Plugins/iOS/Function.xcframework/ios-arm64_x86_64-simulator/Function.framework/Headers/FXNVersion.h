//
//  FXNVersion.h
//  Function
//
//  Created by Yusuf Olokoba on 10/14/2023.
//  Copyright © 2025 NatML Inc. All rights reserved.
//

#pragma once

#include <Function/FXNStatus.h>

#define FXN_VERSION_MAJOR 0
#define FXN_VERSION_MINOR 0
#define FXN_VERSION_PATCH 36

/*!
 @function FXNGetVersion

 @abstract Get the Function version.

 @discussion Get the Function version.

 @returns Function version string.
*/
FXN_API const char* FXNGetVersion ();
