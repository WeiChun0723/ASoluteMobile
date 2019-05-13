using System.Reflection;
using System.Runtime.CompilerServices;
using Android;
using Android.App;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle("HaulageApp.Droid")]
[assembly: AssemblyDescription("Haulage app")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ASolute Sdn Bhd")]
[assembly: AssemblyProduct("ASolute Haulage")]
[assembly: AssemblyCopyright("Copyright ©  ASolute Sdn Bhd 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("1.0.0")]

// The following attributes are used to specify the signing key for the assembly, 
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

[assembly: UsesPermission(Manifest.Permission.AccessFineLocation)]
[assembly: UsesPermission(Manifest.Permission.AccessCoarseLocation)]
[assembly: UsesPermission(Manifest.Permission.Internet)]
[assembly: UsesPermission(Manifest.Permission.ReadExternalStorage)]
[assembly: UsesPermission(Manifest.Permission.WriteExternalStorage)]
[assembly: UsesPermission(Manifest.Permission.Camera)]
[assembly: UsesFeature("android.hardware.camera.autofocus", Required = true)]
