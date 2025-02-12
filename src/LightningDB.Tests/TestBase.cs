using System;
using System.IO;

namespace LightningDB.Tests;

public class TestBase
{
   private static string _tempPath = Path.Combine(Path.GetTempPath(), $"lightningtests-{Environment.Version.ToString()}");
   
   protected string TempPath(string seed = "")
   {
      var path = Path.Combine(_tempPath, $"t{seed}", Guid.NewGuid().ToString());
      Directory.CreateDirectory(path);
      return path;
   }
   protected LightningEnvironment CreateEnvironment(string path = null, EnvironmentConfiguration config = null) => 
      new(path ?? TempPath(), config);
   
   public static void CleanupSession()
   {
      if(Directory.Exists(_tempPath))
         Directory.Delete(_tempPath, true);
   }
}