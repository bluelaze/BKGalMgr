using System;
using System.Collections.Generic;
using System.Text;

namespace BKGalMgr.Enums;

public enum DpiOverrideMode
{
    None = 0, // 清除/恢复默认
    Application = 1, // 应用程序 (~ HIGHDPIAWARE)
    System = 2, // 系统 (~ DPIUNAWARE)
    SystemEnhanced = 3, // 系统 (增强) (~ GDIDPISCALING DPIUNAWARE)
}
