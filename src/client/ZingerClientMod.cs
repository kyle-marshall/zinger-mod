using LogicAPI.Client;
using LogicLog;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ZingerMod.Client {
    public class ZingerClientMod : ClientMod {
        protected override void Initialize() {
            Logger.Info("ZingerMod Initialize");
        }
    }
}