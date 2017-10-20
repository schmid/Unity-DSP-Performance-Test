Setup
=====
- I have set up tests for different approaches to sine computation.
- The same algorithms are being tested with C# and C++ implementations.
- Tests run 144000000 iterations and are timed with StopWatch (C#) or chrono::high_resolution_clock (C++).
- C++ code is compiled with VS2017 in x86 Release mode with optimization enabled
- C# code is compiled with VS2017 in Release mode with optimization enabled



Test Results
============
- All tests were run on an Intel Core i7-6700HQ 2.60 GHz.

Library Sine Test
-----------------

	float sum = 0.0f;
	float angle_per_iteration = PI2 / iterations;
	for (long i = 0; i < iterations; ++i) {
		float angle = i * angle_per_iteration;
		// C++ uses sinf(), C# uses (float)Math.Sin()
		sum += SIN(angle); // make sure that code isn't optimized away
	}

Results (144000000 iterations):
- C++ : 1011 iterations/smp (1 smp = 1/48000 s)
- C#  :  388 iterations/smp 

C++ is 2.6 times faster


Polynomial Approximation Test
-----------------------------

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
- C++ : 7500 iterations/smp 
- C#  : 1066 iterations/smp

C++ is 7 times faster


Array Test
----------
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

Results (144000000 iterations, TABLE_SIZE=2048)
- C++ : 14354 iterations/smp
- C#  :   976 iterations/smp
Results (144000000 iterations, TABLE_SIZE=16M)
- C++ : 14563 iterations/smp
- C#  :   967 iterations/smp

C++ is 15 times faster!



C++ Notes
=========
- For the Array Test at least, the C++ binary is SSE optimized:

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
