Neuroflow (GPL v3) v0.0.2 Alpha 
===============================

A Workflow Foundation based Machine Learning Algorithm Library by using GPGPU for computation backend.

http://neuroflowblog.wordpress.com/

v0.0.2 Description

This is a very basic proof-of-concept implementation of the proposed library. There is a tiny example program for performance testing, and various unit tests for showing and verifying the features.

v0.0.2 Changelog

- Improved kernel compilation time requirement (binary caching implemented)
- Minor performance optimizations

v0.0.2 Features:

- It supports OpenCL CPU and GPU modes, kernels are optimized for each
- Architectures: 
  - Feed Forward Multilayer Perceptron
  - Recurrent Multilayer Perceptron w/ Backpropagation Through Time
  - Recurrent Multilayer Perceptron w/ Realtime Recurrent Learning (no OpenCL support yet)
- Learning algorithms: 
  - Online Gradient Descent
  - Offline Gradient Descent
  - Alopex-B (no OpenCL support yet)
  - Cross Entropy Method (no OpenCL support yet)

v0.0.2 Requirements

- Visual Studio 2013
- Boost 1.55b (BOOST, BOOSTLIBx86, BOOSTLIBx64 environment variable must be set)
- Intel or AMD OpenCL SDK (OCLINC, OCLLIBx86, OCLLIBx64  environment variable must be set)

v0.0.2 Future Directions

- RTLR by using OpenCL CPU and GPU modes
- Multiple activation function support (Tanh, Bipolar Logistic)
- Cross Entropy cost function support
- Sign Changes, SuperSAB, Rprop (all variations), SCG, Oja
- Weight Decay
- Long Short Term Memory architecture
- Workflow Foundation 4.5 Activities. I have proof-of-concept code for WF integration ideas in my private repository but there is nothing publish-ready yet.
- C++ AMP support
- Double precision support
