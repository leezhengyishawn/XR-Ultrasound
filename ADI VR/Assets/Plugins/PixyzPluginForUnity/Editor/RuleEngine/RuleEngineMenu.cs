﻿using UnityEditor;
using Pixyz.Toolbox.Editor;
using Pixyz.Commons.Extensions.Editor;

namespace Pixyz.RuleEngine.Editor
{
    public static class RuleEngineMenu {

        [MenuItem("Pixyz/RuleEngine/Create New Rule Set", priority = 101)]
        [MenuItem("Assets/Create/Pixyz/RuleEngine Rule Set", priority = 1)]
        public static void CreateRuleEngineRules() {
            EditorExtensions.CreateAsset<RuleSet>("New Rule Set");
        }

        [MenuItem("Pixyz/RuleEngine/Create New Custom Action", priority = 102)]
        public static void CreateNewRuleEngineAction() {
            ToolsEditor.CreateNewAction(true, false);
        }
    }
}