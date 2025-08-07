# Muna for Unity Engine

![Muna logo](https://raw.githubusercontent.com/muna-ai/.github/main/logo_wide.png)

[![Dynamic JSON Badge](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fdiscord.com%2Fapi%2Finvites%2Fy5vwgXkz2f%3Fwith_counts%3Dtrue&query=%24.approximate_member_count&logo=discord&logoColor=white&label=Muna%20community)](https://discord.gg/muna)

Run AI inference in your Unity applications.

## Installing Muna
Add the following items to your Unity project's `Packages/manifest.json`:
```json
{
  "scopedRegistries": [
    {
      "name": "Muna",
      "url": "https://registry.npmjs.com",
      "scopes": ["ai.muna"]
    }
  ],
  "dependencies": {
    "ai.muna.muna": "0.0.42"
  }
}
```

## Retrieving your Access Key
Head over to [muna.ai](https://muna.ai) to create an account by logging in. Once you do, generate an access key:

![generate access key](https://raw.githubusercontent.com/muna-ai/.github/main/access_key.gif)

Then add it to your Unity project in `Project Settings > Muna`:

![add access key to Unity](settings.gif)

> [!CAUTION]
> If your Unity project is open-source, make sure to add `UserSettings/` to your `.gitignore` file to keep your Muna access key private.

## Making a Prediction
First, create a Muna client:
```csharp
using Muna;

// 💥 Create a Muna client
var muna = MunaUnity.Create();
```

Then make a prediction:
```csharp
// 🔥 Make a prediction
var prediction = await muna.Predictions.Create(
    tag: "@fxn/greeting",
    inputs: new () { ["name"] = "Roberta" }
);
```

Finally, use the results
```csharp
// 🚀 Use the results
Debug.Log(prediction.results[0]);
```

## Requirements
- Unity 2022.3+

## Supported Platforms
- Android API Level 24+
- iOS 14+
- macOS 12+ (Apple Silicon and Intel)
- Windows 10+ (64-bit only)
- WebGL:
  - Chrome 91+
  - Firefox 90+
  - Safari 16.4+

## Useful Links
- [Discover predictors to use in your apps](https://muna.ai/explore).
- [Join our Discord community](https://discord.gg/muna).
- [Check out our docs](https://docs.muna.ai).
- Learn more about us [on our blog](https://blog.muna.ai).
- Reach out to us at [hi@muna.ai](mailto:hi@muna.ai).

Thank you very much!
