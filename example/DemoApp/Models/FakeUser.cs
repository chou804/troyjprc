using System;
using System.Collections.Generic;
using System.Text;
using TroyLab.JRPC;

namespace DemoApp.Models
{
    public class FakeUser : AuthUser
    {
        public int UserId { get; set; }
    }
}
