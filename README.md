aclib
=====

The Advanced Console Library. A wrapper for the Windows Console API to expose more advanced console features in C#

====

ACLib uses Windows API calls (via pinvoke) to provide helper functions and lower-level access to the Windows Console. It was created to meet a demand for more advanced functionality in a clean manner.

ACLib allows you to:

  * Handle the output buffer in a manner similar to SDL or OpenGL, providing several helper functions for drawing.
  * Access the input buffer in a clean, user-friendly manner that allows access to any key press, as well as mouse information, without locking the thread that calls it.

====

Quick code examples can be observed in the TestApp project, maily in the 'Engine' class and the 'State' classes under the 'States' directory.

Full documentation and tutorials will come soon.

====

DISCLAIMER: aclib is still in development, and may be buggy in certain areas. Certain features of aclib are subject to change. Use at your own risk.
