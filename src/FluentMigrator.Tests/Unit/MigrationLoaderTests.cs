#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Integration.Migrations;
using FluentMigrator.VersionTableInfo;
using NUnit.Framework;
using NUnit.Should;
using System.Linq;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class MigrationLoaderTests
	{
		[Test]
		public void CanFindMigrationsInAssembly()
		{
			var conventions = new MigrationConventions();
			var loader = new MigrationLoader(conventions);
			var asm = Assembly.GetExecutingAssembly();
			IEnumerable<MigrationMetadata> migrationList = loader.FindMigrationsIn(asm, string.Empty);

			//if this works, there will be at least one migration class because i've included on in this code file
			var en = migrationList.GetEnumerator();
			int count = 0;
			while (en.MoveNext())
				count++;

			count.ShouldBeGreaterThan(0);
		}

		[Test]
		public void CanFindMigrationsInNamespace()
		{
			var conventions = new MigrationConventions();
			var loader = new MigrationLoader(conventions);
			var asm = Assembly.GetExecutingAssembly();
			var migrationList = loader.FindMigrationsIn(asm, "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass1");
			migrationList.Select(x => x.Type).ShouldNotContain(typeof(VersionedMigration));
			migrationList.Count().ShouldBeGreaterThan(0);
		}

		[Test]
		public void CanLoadDefaultVersionTableMetaData()
		{
			var conventions = new MigrationConventions();
			var loader = new MigrationLoader(conventions);
			var asm = Assembly.GetExecutingAssembly();
			IVersionTableMetaData versionTableMetaData = loader.GetVersionTableMetaData(asm);
			versionTableMetaData.ShouldBeOfType<TestVersionTableMetaData>();
		}

		[Test]
		public void CanLoadCustomVersionTableMetaData()
		{
			var conventions = new MigrationConventions();
			var loader = new MigrationLoader(conventions);
			var asm = GetAssemblyWithCustomVersionTableMetaData();
			IVersionTableMetaData versionTableMetaData = loader.GetVersionTableMetaData(asm);
			Assert.AreEqual(TestVersionTableMetaData.TABLENAME,versionTableMetaData.TableName);
			Assert.AreEqual(TestVersionTableMetaData.COLUMNNAME, versionTableMetaData.ColumnName);
		}


		/// <summary>
		/// Creates an assembly by dynamically compiling TestVersionTableMetaData.cs
		/// </summary>
		/// <returns></returns>
		private Assembly GetAssemblyWithCustomVersionTableMetaData()
		{
			CodeDomProvider provider = new Microsoft.CSharp.CSharpCodeProvider();

			CompilerParameters parms = new CompilerParameters();

			// Configure parameters
			parms.GenerateExecutable = false;
			parms.GenerateInMemory = true;
			parms.IncludeDebugInformation = false;
			parms.ReferencedAssemblies.Add("System.dll");
			parms.ReferencedAssemblies.Add("FluentMigrator.dll");

			CompilerResults results = provider.CompileAssemblyFromFile(parms, "..\\..\\Unit\\TestVersionTableMetaData.cs");
			Assert.AreEqual(0, results.Errors.Count);
			return results.CompiledAssembly;
		}
	}
}

