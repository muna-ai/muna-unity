/* 
*   Muna
*   Copyright © 2026 NatML Inc. All rights reserved.
*/

namespace Muna.Tests {

    using UnityEngine;
    using C;

    public class JsonTest : MonoBehaviour {

        struct Pet {
            public string sound;
            public int legs;
        }

        void Start() {
            var pet = new Pet { sound = "woof", legs = 6 };
            var petJson = Json.From(pet);
            var petValue = Value.CreateDict(petJson);
            Debug.Log($"Pet JSON: {petJson}");
            Debug.Log($"Pet deserialized JSON: {petValue.ToObject()}");
        }
    }
}