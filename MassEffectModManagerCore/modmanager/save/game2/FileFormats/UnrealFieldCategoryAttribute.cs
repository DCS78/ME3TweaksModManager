﻿using System;

namespace ME3TweaksModManager.modmanager.save.game2.FileFormats
{
    public class UnrealFieldCategoryAttribute : Attribute
    {
        public string Category;

        public UnrealFieldCategoryAttribute(string category)
        {
            this.Category = category;
        }
    }
}
