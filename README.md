Goal and Key Results
====================
This documents a series of tests for different approaches to sine computation, the goal being to test the relative performance of different approaches to DSP code for use in Unity. This would be relevant for writing realtime software synthesizers and effects for Unity games. The example used is a simple algorithm that computes a sum of sines using different approaches: library functions, polynomial approximation, and lookup tables of different sizes.

The key result:

**C# in Unity with the IL2CPP scripting backend performs comparably to C++**,

which indicates that it could be viable for implementing DSP code.

The following graph shows the performance of 4 different implementations (lookup tables are the fastest) implemented using C++ and C# with different scripting backends.

![results chart](https://raw.githubusercontent.com/schmid/Unity-DSP-Performance-Test/master/sine_perf.png)

In Unity PlayerSettings, the IL2CPP scripting backend is enabled here:

![Unity PlayerSettings](https://raw.githubusercontent.com/schmid/Unity-DSP-Performance-Test/master/PlayerSettings.png)

Setup
=====
- The same algorithms are being tested with C# and C++ implementations.
- Tests run 144000000 iterations and are timed with `StopWatch` (C#) or `chrono::high_resolution_clock` (C++).
- C++ code is compiled with VS2017 in Release mode with optimization enabled
- .NET C# code is compiled with VS2017 in Release mode with optimization enabled
- 'Unity2017' means compiled with Unity 2017.2.0f3 (64-bit)
  - Unity2017-Editor is running in editor (Mono)
  - Unity2017-3.5 is Unity standalone with Scripting Runtime Version is set to '.NET 3.5 equivalent' (Mono)
  - Unity2017-4.6 is Unity standalone with Scripting Runtime Version is set to '.NET 4.6 equivalent' (Mono)
- 'Unity2018-IL2CPP-4.x' means Unity2018.2.5f1 standalone, Backend: IL2CPP, .NET 4.x Equivalent, .NET 4.x, Compiled in Visual Studio 2017, 'Release' build, - 'Master' did not have any significant increase in performance.
- All tests were run on an Intel Core i7-6700HQ 2.60 GHz.



Overview of Results
===================
- C++ (x64), C# (.NET), and C# compiled with Unity's IL2CPP perform comparably, other Unity C# scripting backends and .NET with 'Prefer 32-bit' enabled perform much worse, in most tests by an order of magnitude.
- The fastest tested approach to sine computation was a look-up table (LUT). Tables with 2K to 16M floats performed equally well. This was a surprise to me, since the i7-6800HQ only has 6 MB of cache.
- C# (.NET) outperforms C# (Unity) by a factor of 1.2-17, depending on the test type, however, [according to official Unity forum posts, .NET is deprecated in favour of Mono and IL2CPP](https://forum.unity.com/threads/deprecation-of-support-for-the-net-scripting-backend-used-by-the-universal-windows-platform.539685/).
- If 'Prefer 32-bit' is enabled in VS2017, C# performs *very poorly*, up to 16x slower.


Test Results
============


Library Sine Test
-----------------

Code:

    float sum = 0.0f;
    float angle_per_iteration = PI2 / iterations;
    for (long i = 0; i < iterations; ++i) {
        float angle = i * angle_per_iteration;
        // C++ uses sinf(), C# uses (float)Math.Sin()
        sum += SIN(angle); // make sure that code isn't optimized away
    }

Results (144000000 iterations):
```
- C++ (x86)                 :  48528 K iter/s
- C++ (x64)                 : 275856 K iter/s !
- C# (Unity2017-3.5)        :  21216 K iter/s 
- C# (Unity2017-4.6)        :  21264 K iter/s 
- C# (Unity2017-Editor)     :  20400 K iter/s 
- C# (.NET)                 :  24048 K iter/s 
- C# (.NET Prefer 32-bit)   :  18624 K iter/s 
- C# (Unity2018-IL2CPP-4.x) : 102345 K iter/s
```


Polynomial Approximation Test
-----------------------------

Code:

    float sum = 0.0f;
    const float periods_per_iteration = 1.0f / iterations;
    const float F = -8.0f * PI;
    for (long i = 0; i < iterations; ++i) {
        float p = i * periods_per_iteration;
        // C++ uses abs, C# uses Math.Abs
        float sine = 8 * p + F * p * ABS(p * PI2);
        sum += sine;
    }

Results (144000000 iterations):
```
- C++ (x86)                 : 396672 K iter/s 
- C++ (x64)                 : 429840 K iter/s 
- C# (Unity2017-3.5)        :  34416 K iter/s 
- C# (Unity2017-4.6)        :  25920 K iter/s 
- C# (Unity2017-Editor)     :  33072 K iter/s
- C# (.NET)                 : 571440 K iter/s !
- C# (.NET Prefer 32-bit)   :  51168 K iter/s
- C# (Unity2018-IL2CPP-4.x) : 401114 K iter/s
```


Array Test
----------

Code::

    // C++ uses 'float *', C# uses 'float[]'
    FLOAT_ARRAY sine = new float[TABLE_SIZE];
    float sum = 0.0f;
    const float periods_per_iteration = 1.0f / iterations;
    for (long i = 0; i < iterations; ++i)
    {
        float periods = i * periods_per_iteration;
        int idx = periods * TABLE_SIZE;
        sum += sine[idx];
    }

Results (144000000 iterations, `TABLE_SIZE`=2048)
```
- C++ (x86)                 : 770016 K iter/s
- C++ (x64)                 : 778368 K iter/s !
- C# (Unity2017-3.5)        :  76224 K iter/s 
- C# (Unity2017-4.6)        :  77472 K iter/s 
- C# (Unity2017-Editor)     :  68880 K iter/s
- C# (.NET)                 : 746112 K iter/s
- C# (Prefer 32-bit)        :  46848 K iter/s
- C# (Unity2018-IL2CPP-4.x) : 651584 K iter/s
```

Results (144000000 iterations, `TABLE_SIZE`=16M)
```
- C++ (x86)                 : 761904 K iter/s !
- C++ (x64)                 : 757872 K iter/s
- C# (Unity2017-3.5)        :  80736 K iter/s 
- C# (Unity2017-4.6)        :  77472 K iter/s 
- C# (Unity2017-Editor)     :  69984 K iter/s
- C# (.NET)                 : 746112 K iter/s
- C# (.NET Prefer 32-bit)   :  46416 K iter/s
- C# (Unity2018-IL2CPP-4.x) : 645740 K iter/s
```

C++ Notes
=========
- For the Array Test at least, the C++ binary is SSE optimized:
```
            // Array Test inner loop:

            float periods = i * periods_per_iteration;
    00007FF62B02C8BE  cvtsi2ss    xmm0,dword ptr [rbp+44h]  
    00007FF62B02C8C3  mulss       xmm0,dword ptr [periods_per_iteration]  
    00007FF62B02C8C8  movss       dword ptr [rbp+64h],xmm0  
            int idx = periods * table_size;
    00007FF62B02C8CD  mov         rax,qword ptr [this]  
    00007FF62B02C8D4  cvtsi2ss    xmm0,dword ptr [rax+38h]  
    00007FF62B02C8D9  movss       xmm1,dword ptr [rbp+64h]  
    00007FF62B02C8DE  mulss       xmm1,xmm0  
    00007FF62B02C8E2  movaps      xmm0,xmm1  
    00007FF62B02C8E5  cvttss2si   eax,xmm0  
    00007FF62B02C8E9  mov         dword ptr [rbp+84h],eax  
            sum += sine[idx];
    00007FF62B02C8EF  movsxd      rax,dword ptr [rbp+84h]  
    00007FF62B02C8F6  mov         rcx,qword ptr [this]  
    00007FF62B02C8FD  mov         rcx,qword ptr [rcx+40h]  
    00007FF62B02C901  movss       xmm0,dword ptr [sum]  
    00007FF62B02C906  addss       xmm0,dword ptr [rcx+rax*4]  
    00007FF62B02C90B  movss       dword ptr [sum],xmm0   
```
