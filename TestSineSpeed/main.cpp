#define _USE_MATH_DEFINES
#include <iostream>
#include <cmath>
#include <chrono>
#include <string>
#include <sstream>
#include <conio.h>
#include <vector>

const long ITERATIONS = 48000L * 3000L;

using std::string;
using std::vector;
using std::cout;
using std::endl;

class Test
{
public:
	const string name;
	Test(const string& name) : name(name) {}
	virtual ~Test() {}
	virtual void init() = 0;
	virtual float perform_test(long iterations) = 0;
};

class Test_sinf : public Test
{
public:
	Test_sinf(const string &name) : Test(name) {}
	virtual ~Test_sinf() {}
protected:
	void init();
	float perform_test(long iterations);
	const string generate_results() const;
};

void Test_sinf::init() { }

float Test_sinf::perform_test(long iterations)
{
	float sum = 0.0f;
	float angle_per_iteration = M_2_PI / iterations;
	for (long i = 0; i < iterations; ++i)
	{
		float angle = i * angle_per_iteration;
		sum += sinf(angle); // make sure that code isn't optimized away
	}
	return sum;
}

class Test_table : public Test
{
public:
	Test_table(const string &name, int table_size) : Test(name), table_size(table_size) {}
	virtual ~Test_table() {}
protected:
	void init();
	float perform_test(long iterations);
	const string generate_results() const;
private:
	int table_size = -1;
	float *sine;
};

void Test_table::init()
{
	sine = new float[table_size];
	float angle_per_iteration = M_2_PI / table_size;
	for (int i = 0; i < table_size; ++i)
	{
		sine[i] = sinf(i * angle_per_iteration);
	}
}

float Test_table::perform_test(long iterations)
{
	float sum = 0.0f;
	const float periods_per_iteration = 1.0f / iterations;
	for (long i = 0; i < iterations; ++i)
	{
		float periods = i * periods_per_iteration;
		int idx = periods * table_size;
		sum += sine[idx];
	}
	return sum;
}

class Test_parabolic : public Test
{
public:
	Test_parabolic(const string &name) : Test(name) {}
	virtual ~Test_parabolic() {}
protected:
	void init();
	float perform_test(long iterations);
	const string generate_results() const;
};

void Test_parabolic::init() { }


// From Nicolas Capens: http://forum.devmaster.net/t/fast-and-accurate-sine-cosine/9648
// float sine(float x)
// {
//     const float B = 4/pi;
//     const float C = -4/(pi*pi);
//     float y = B * x + C * x * abs(x);
//     #ifdef EXTRA_PRECISION
//     //  const float Q = 0.775;
//         const float P = 0.225;
//         y = P * (y * abs(y) - y) + y;   // Q * y + P * y * abs(y)
//     #endif
// }
//
// So, for the inaccurate version we have:
//   B = 4/pi
//   C = -4/(pi*pi)
//   y = B x + C x abs(x)
//
// We can use periods instead of degrees:
//   p * 2 pi = x
//   y = (4/pi) * x + (-4/(pi*pi)) * x * abs(x)
//     = 4 / pi * x + -4 / pi * pi * x * abs(x)
//     = 4 / pi * p * 2 * pi + -4 / pi * pi * p * 2 * pi * abs(p * 2 * pi)
//     = 4 * p * 2 + -4 * p * 2 * pi * abs(p * 2 * pi)
//     = 8 * p + -8 * pi * p * abs(p * 2 * pi)
//     = 8p + -8pi * p * abs(p * 2pi)
float Test_parabolic::perform_test(long iterations)
{
	float sum = 0.0f;
	const float periods_per_iteration = 1.0f / iterations;
	const float F = -8.0f * M_PI;
	for (long i = 0; i < iterations; ++i)
	{
		float p = i * periods_per_iteration;
		float sine = 8 * p + F * p * abs(p * M_2_PI);
		sum += sine;
	}
	return sum;
}

void run(Test &test, long iterations)
{
	test.init();
	cout << test.name << ":" << endl;
    using milli = std::chrono::milliseconds;
    auto start = std::chrono::high_resolution_clock::now();
	float result = test.perform_test(iterations);
    auto finish = std::chrono::high_resolution_clock::now();
	auto time_ms = std::chrono::duration_cast<milli>(finish - start).count();
	float time_s = time_ms * 0.001f;
	cout << "  " << (iterations/48000) / time_s << " sines/smp ("
		 << iterations << " iterations) (sum = " << result << ")" << std::endl;
}

int main()
{
	Test_sinf test_sinf("Library Sine Test");
	Test_parabolic test_parabolic("Polynomial Approximation Test");
	Test_table test_table_2048("Array Test (2048 samples)", 2048);
	//Test_table test_table_64K("table test (64K samples)", 1 << 16);
	Test_table test_table_16M("Array Test (16M samples)", 1 << 24);
	vector<Test *> tests{ &test_sinf, &test_parabolic, &test_table_2048, /*&test_table_64K,*/ &test_table_16M };

	for(Test *test : tests)
	{
		run(*test, ITERATIONS);
	}

	//cout << "done. (press 'any' key)" << endl;
	//getch();
	return 0;
}
