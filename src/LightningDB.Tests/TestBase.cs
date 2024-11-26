using System;
using Xunit;

namespace LightningDB.Tests;

[Collection("SharedFileSystem")]
public class TestBase : IDisposable
{
   private readonly SharedFileSystem _fileSystem;
   protected LightningEnvironment _env;
   
   protected TestBase(SharedFileSystem fileSystem, bool createEnvironment = true)
   {
      _fileSystem = fileSystem;
      if(createEnvironment)
         CreateEnvironment();
   }

   protected string TempPath(string seed = "") => _fileSystem.CreateNewDirectoryForTest(seed);
   protected void CreateEnvironment(string path = null, EnvironmentConfiguration config = null) => _env = new LightningEnvironment(path ?? TempPath(), config);

   public void Dispose()
   {
      Dispose(true);
   }

   protected virtual void Dispose(bool disposing)
   {
      _env?.Dispose();
      _env = null;
   }
}