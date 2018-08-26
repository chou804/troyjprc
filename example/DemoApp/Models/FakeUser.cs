using System;
using System.Collections.Generic;
using System.Text;
using TroyLab.JRPC.Core;

namespace DemoApp.Models
{
    public class FakeUser : AuthUser
    {
        public int UserId { get; set; }
    }
}
