﻿using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryMSystem
{
    public delegate void deleUpdateContorl(string s);
    public delegate void deleUpdateCommContorl(string type, object o, string s);
    public delegate void deleControlInvoke(object o);

    public delegate void deleRfidOperateCallback(object o);
    public delegate void deleControlInvokeVoid_Void();
}
