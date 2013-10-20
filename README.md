Neuroflow 0.0.1 (GPL v3)
=========================

A Workflow Foundation based Machine Learning Algorithm Library by using GPGPU for computation backend.

0.0.1 Features:

This is a very basic proof-of-concept implementation of the proposed library. There is a tiny example program for performance testing, and various unit tests for showing and verifying the features.

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

0.0.1 Requirements

- Visual Studio 2013
- Boost 1.53+ (BOOST environment variable must be set to Boost path)
- Intel OpenCL SDK (in a future release this might be a generic OpenCL SDK requirement)

0.0.1 Future Directions

- RTLR by using OpenCL CPU and GPU modes
- Multiple activation function support (Tanh, Bipolar Logistics)
- Cross Entropy cost function support
- Sign Changes, SuperSAB, Rprop (all variations), SCG, Oja
- Weight Decay
- Long Short Term Memory architecture
- Workflow Foundation 4.5 Activities. I have proof-of-concept code for WF integration ideas in my private repository but there is nothing publish-ready yet.
- C++ AMP support
- Double precision support
