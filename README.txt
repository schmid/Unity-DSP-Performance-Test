Setup
=====
- For these experiments, I have assumed that 1 smp is is 1/48000 s
- C++ code is compiled with VS2017 in x86 Release mode with optimization enabled
- C# code is compiled with VS2017 in Release mode with optimization enabled

Library Sine Test
=================

	float sum = 0.0f;
	float angle_per_iteration = PI2 / iterations;
	for (long i = 0; i < iterations; ++i) {
		float angle = i * angle_per_iteration;
		// C++ uses sinf(), C# uses (float)Math.Sin()
		sum += SIN(angle); // make sure that code isn't optimized away
	}

Results (144000000 iterations):
- C++ : 1011 iterations/smp
- C#  :  388 iterations/smp 

C++ is 2.6 times faster

Polynomial Approximation Test
=============================

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
==========
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
