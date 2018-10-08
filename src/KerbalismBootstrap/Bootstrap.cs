﻿/*
 * Thanks to blowfish for guiding me through this!
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalismBootstrap
{
	[KSPAddon( KSPAddon.Startup.Instantly, false )]
	public class Bootstrap : MonoBehaviour
	{

		public void Start()
		{
			if (Util.FindKerbalism() != null)
				print( "[KerbalismBootstrap] WARNING: KERBALISM HAS ALREADY LOADED BEFORE US!" );

			string our_bin = Path.Combine( AssemblyDirectory( Assembly.GetExecutingAssembly() ), Util.BinName + ".bin" );
			string possible_dll = Path.Combine( AssemblyDirectory( Assembly.GetExecutingAssembly() ), "Kerbalism.dll" );

			if (File.Exists( our_bin ))
			{
				print( "[KerbalismBootstrap] Found Kerbalism bin file at '" + our_bin + "'" );
				if (File.Exists( possible_dll ))
				{
					File.Delete( possible_dll );
					print( "[KerbalismBootstrap] Deleted non-bin DLL at '" + possible_dll + "'" );
				}
			}
			else
			{
				print( "[KerbalismBootstrap] ERROR: COULD NOT FIND KERBALISM BIN FILE (" + Util.BinName + ".bin" + ")! Ditching!" );
				return;
			}

			AssemblyLoader.LoadPlugin( new FileInfo( our_bin ), our_bin, null );
			AssemblyLoader.LoadedAssembly loadedAssembly = Util.FindKerbalism();
			if (loadedAssembly == null)
			{

				print( "[KerbalismBootstrap] Kerbalism failed to load! Ditching!" );
				return;
			}
			else
			{
				print( "[KerbalismBootstrap] Kerbalism loaded!" );
			}

			loadedAssembly.Load();

			foreach (Type type in loadedAssembly.assembly.GetTypes())
			{
				foreach (Type loadedType in AssemblyLoader.loadedTypes)
				{
					if (loadedType.IsAssignableFrom( type ))
					{
						loadedAssembly.types.Add( loadedType, type );
						loadedAssembly.typesDictionary.Add( loadedType, type );
					}
				}

				if (type.IsSubclassOf( typeof( MonoBehaviour ) ))
				{
					KSPAddon addonAttribute = (KSPAddon) type.GetCustomAttributes( typeof( KSPAddon ), true ).FirstOrDefault();
					if (addonAttribute != null && addonAttribute.startup == KSPAddon.Startup.Instantly)
					{
						AddonLoaderWrapper.StartAddon( loadedAssembly, type, addonAttribute, KSPAddon.Startup.Instantly );
					}
				}
			}
		}

		public string AssemblyDirectory( Assembly a )
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder( codeBase );
			string path = Uri.UnescapeDataString( uri.Path );
			return Path.GetDirectoryName( path );
		}
	}
}
