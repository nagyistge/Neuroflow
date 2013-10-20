Neuroflow 0.0.1 (GPL v3)
=========================

A Workflow Foundation based Machine Learning Algorithm Library by using GPGPU for computation backend.

This is a very basic proof-of-concept implementation of the proposed library. There is a tiny example program for performance testing, and various unit test for showing and verifying the features.

0.0.1 Features:

- It supports OpenCL CPU and GPU modes, kernels are optimized for each
- Architectures: 
  - Feed Forward Multilayer Perceptron
  - Recurrent Multilayer Perceptron w/ Backpropagation Through Time
  - Recurrent Multilayer Perceptron w/ Realtime Recurrent Learning (no OpenCL support yet)
- Learning algorithms:
  - Online Gradient Descent
  - Offline Gradient Descent
  - Alopex-B
  - Cross Entropy Method

0.0.1 Requirements

- Visual Studio 2013
- Boost 1.53+ (BOOST environment variable must be set to Boost path)
- Intel OpenCL SDK (in a future release this might be a generic OpenCL SDK requirement)

0.0.1 Future Directions

- RTLR by using OpenCL CPU and GPU modes
- Sign Changes, Rprop, SCG
- Long Short Term Memory architecture
- Workflow Foundation 4.5 Activities. I have proof-of-concept code for WF integration ideas in my private repository but there is nothing publish ready here yet, sorry.
- C++ AMP support
