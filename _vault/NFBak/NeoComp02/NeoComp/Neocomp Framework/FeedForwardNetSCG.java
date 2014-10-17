/*
*    FeedForwardNetSCG  --  Feed forward network with scaled conjugate gradient learning.
*
*    Copyright (C) 1999 by Joseph A. Huwaldt <jhuwaldt@gte.net>.  All rights reserved.
*
*    This library is free software; you can redistribute it and/or
*    modify it under the terms of the GNU Library General Public
*    License as published by the Free Software Foundation; either
*    version 2 of the License, or (at your option) any later version.
*
*    This library is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
*    Library General Public License for more details.
*/
package jahuwaldt.tools.NeuralNets;


/**
*  <p>  Feed Forward type network with Scaled Conjugate
*       Gradient learning.  This algorithm avoids an
*       expensive line search by using a Levenberg-Marquardt
*       approach to scale the step size.  This method
*       requires orders of magnitude fewer iterations than
*       backpropogation, but is also vastly more complex
*       computationally and uses 6 times as much memory
*       during training.
*  </p>
*
*  <p>  The SCG algorithm used here has been ported to Java
*       from a C version written by Bruno Orsier (9/95) and
*       included in the Stuttgart Neural Network Simulator
*       version 4.1.  The algorithm itself is documented in:
*       Martin F. M¿ller, "A Scaled Conjugate Gradient 
*       Algorithm for Fast Supervised Learning",
*       November 13, 1990.  I have modified Bruno's code
*       to make the algorithm more tolerant of failure
*       conditions (by restarting the solution process
*       with a new random set of weights when such a
*       failure is encountered).
*  </p>
*
*  <p>  Modified by:  Joseph A. Huwaldt    </p>
*
*  @author    Joseph A. Huwaldt    Date:  June 26, 1999
*  @version   August 11, 2002
**/
public class FeedForwardNetSCG extends FeedForwardNet {

	// Debug flag
	private static final  boolean DEBUG = false;

	// Some constants used by the SCG algorithm.
	protected static final  float FIRST_SIGMA = 1.E-4F;

	protected static final  float FIRST_LAMBDA = 1.E-6F;

	protected static final  float TOLERANCE = 1.E-8F;

	protected static final  float MAXFLOAT = Float.MAX_VALUE / 100;


	/**
	*  Constructor for a feed forward network with Scaled Conjugate
	*  Gradient (SCG) learning where the weights are initially set to
	*  random values (with a gaussian distribution between -5 and +5).
	*
	*  @param  numInputs   Number of input nodes or neurons.
	*  @param  numOutputs  Number of output nodes.
	*  @param  numHLayers  Number of hidden layers in the network.
	*  @param  neuronsPerHLayer  Number of neurons per hidden layer.
	*  @param  factory     Factory object used to generate neurons of
	*                      a user specified type.
	**/
	public FeedForwardNetSCG( int numInputs, int numOutputs, int numHLayers,
										int neuronsPerHLayer, NeuronFactory factory ) {
		super( numInputs, numOutputs, numHLayers, neuronsPerHLayer, factory );
	}

	/**
	*  Train a network to match a set of input patterns to a set
	*  of output patterns.  Uses the scaled conjugate gradient
	*  method for learning.
	*
	*  @param  testGenerator   Training instance generator that
	*          generates combinations of inputs and corresponding
	*          outputs to train the network to reproduce.
	*  @param  tol  Tolerance to train network to (mean error across
	*          all the outputs accross a training set).
	*  @param  maxIterations  Max number of iterations allowed for
	*          training.
	*  @return Number of training interations completed, if this number
	*          is >= maxIterations, then a solution was not found.
	**/
	public int train( TrInstGenerator testGenerator, float tol, int maxIterations ) {
		// Number of iterations completed.
		int iter = 1;
		
		// Looping variables.
		int i, k = 1;
		
		//	Tolerance squared.
		float tol2 = tol*tol;
		
		// SCG parameters.
		boolean success = true;
		float delta = 0;
		float lambda_bar = 0, lambda = FIRST_LAMBDA;
		float old_error = 0;
		float mag_of_p_2 = 0;
		boolean under_tolerance;
		int count_under_tol = 0;
		boolean restart = false;
		
		// Calculate the initial error in the network.
		float error = MSError( testGenerator );
		if ( DEBUG )
			System.out.println( "iter = " + 0 + ", error=" + error );
		
		//	If the net is already converged, just return.
		if ( error <= tol2 )
			return 0;
		
		// Start with existing set of connection weights.
		float[] weights = getAllWeights();
		int numWeights = weights.length;
		
		// Allocate memory to hold weight gradients.
		float[] gradient = new float [ numWeights ];
		
		// Step direction vector.
		float[] p = new float [ numWeights ];
		float[] r = new float [ numWeights ];
		
		// Storage space for previous iteration values.
		float[] old_weights = new float [ numWeights ];
		float[] old_gradient = new float [ numWeights ];
		
		// Calculate the starting set of gradients.
		calcGradients( testGenerator, gradient );
		
		// Initialize p & r vectors to be equal to -gradient.
		for ( i = 0; i < numWeights; ++i )
			p[i] = r[i] = -gradient[i];
		
		// Main part of the SCG algorithm.
		do {
			if ( restart ) {
				// First time through, set initial values for SCG parameters.
				lambda = FIRST_LAMBDA;
				lambda_bar = 0;
				k = 1;
				count_under_tol = 0;
				success = true;
				restart = false;
			}
			
			float mag_of_rk = vectorMagnitude( r );
			
			// If an error reduction is possible, calculate 2nd order info.
			if ( success ) {
				
				// If the search direction is small, stop.
				mag_of_p_2 = vectorProduct( p, p );
				if ( mag_of_p_2 <= TOLERANCE*TOLERANCE ) {
					if ( DEBUG )
						System.out.println( "mag_of_p_2 is <= TOLERANCE*TOLERANCE" );
					
					// If we fail, then try resetting the
					// network weights to random values and starting over.
					setRandomWeights();
					weights = getAllWeights();
					
					count_under_tol = 0;
					lambda = FIRST_LAMBDA;
					lambda_bar = 0;
					k = 1;
					count_under_tol = 0;
					success = true;
					restart = false;
					++iter;
					error = MSError( testGenerator );
					calcGradients( testGenerator, gradient );
					for ( i = 0; i < numWeights; ++i )
						p[i] = r[i] = - gradient[i];
					
					mag_of_rk = vectorMagnitude( r );
					mag_of_p_2 = vectorProduct( p, p );
					
					// If it fails a second time, bail out.
					if ( mag_of_p_2 <= TOLERANCE*TOLERANCE ) {
						if ( DEBUG )
							System.out.println( "mag_of_p_2 is <= TOLERANCE*TOLERANCE" );
						
						iter = maxIterations;
						break;
					}
				}
				
				float sigma = FIRST_SIGMA/(float)Math.sqrt( mag_of_p_2 );
				
				// In order to compute the new step, we need a new gradient.
				// First, save off the old data.
				arrayCopy( gradient, old_gradient );
				arrayCopy( weights, old_weights );
				old_error = error;
				
				// Now we move to the new point in weight space.
				for ( i = 0; i < numWeights; ++i )
					weights[i] += sigma*p[i];
				
				setAllWeights( weights );
				
				// And compute the new gradient.
				calcGradients( testGenerator, gradient );
				
				// Now we have the new gradient, and we continue the step computation.
				delta = 0;
				for ( i = 0; i < numWeights; ++i ) {
					float step = (gradient[i] - old_gradient[i])/sigma;
					delta += p[i]*step;
				}
			}
			
			// Scale delta.
			delta += (lambda - lambda_bar)*mag_of_p_2;
			
			// If delta <= 0, make Hessian positive definite.
			if ( delta <= 0 ) {
				lambda_bar = 2*(lambda - delta/mag_of_p_2);
				delta = lambda*mag_of_p_2 - delta;
				lambda = lambda_bar;
			}
			
			// Calculate step size.
			float mu = vectorProduct( p, r );
			float alpha = mu/delta;
			
			// Calculate the comparison parameter.
			// We must compute a new gradient, but this time we do not
			// want to keep the old values.  They were useful only for
			// approximating the Hessian.
			for ( i = 0; i < numWeights; ++i )
				weights[i] = old_weights[i] + alpha*p[i];
			
			setAllWeights( weights );
			
			// Calculate the new error & gradients of the network.
			error = MSError( testGenerator );
			calcGradients( testGenerator, gradient );
			if ( DEBUG )
				System.out.println( "iter = " + iter + ", error=" + error );
			
			float gdelta = 2*delta*(old_error - error)/(mu*mu);
			
			// If gdelta >= 0, a sucessful reduction in error is possible.
			if ( gdelta >= 0 ) {
				// Product of r(k+1) by r(k)
				float rsum = 0;
				
				// Are we under tolerance?
				under_tolerance = 2*Math.abs( old_error - error ) <=
												TOLERANCE*(Math.abs( old_error ) + Math.abs( error ) + 1.E-10F);
				
				// We are already at w(k+1) in weight space, so we don't need to move.
				// We compute |r(k)| before changing r to r(k+1).
				mag_of_rk = vectorMagnitude( r );
				
				// Now r = r(k+1).
				for ( i = 0; i < numWeights; ++i ) {
					float tmp = - gradient[i];
					rsum += tmp*r[i];
					r[i] = tmp;
				}
				lambda_bar = 0;
				success = true;
				
				// Do we need to restart?
				if ( k >= numWeights ) {
					restart = true;
					arrayCopy( r, p );
					
				} else {
					// Compute new conjugate direction.
					float beta = (vectorProduct( r, r ) - rsum)/mu;
					
					// Update direction vector.
					for ( i = 0; i < numWeights; ++i )
						p[i] = r[i] + beta*p[i];
					
					restart = false;
				}
				
				if ( gdelta >= 0.75F )
					lambda *= 0.25F;		// lambda = lambda/4.0
				
			} else {
				// A reduction in error was not possible.
				under_tolerance = false;
				
				// Go back to w(k) since w(k) + alpha*p(k) is not better.
				arrayCopy( old_weights, weights );
				error = old_error;
				lambda_bar = lambda;
				success = false;
			}
			
			if ( gdelta < 0.25F )
				lambda += delta*(1 - gdelta) / mag_of_p_2;
			
			// Try to prevent floating point overflows.  Lambda may become
			// too big even with the under_tolerance criterion if there are
			// several consecutive "NO REDUCTIONs".
			if ( lambda > MAXFLOAT )
				lambda = MAXFLOAT;
			
			// The SCG stops after 3 consecutive under_tolerance steps.
			if ( under_tolerance )
				++count_under_tol;
			else
				count_under_tol = 0;
			
			// Check for failure conditions.
			if ( count_under_tol > 2 || mag_of_rk <= TOLERANCE ) {
				// If we fail, then try resetting the
				// network weights to random values and starting over.
				if ( DEBUG ) {
					if ( count_under_tol > 2 )
						System.out.println( "count_under_tol is > 2" );
					else
						System.out.println( "norm_of_rk is <= tolerance" );
				}
				
				// Reset the weights to random values and try again.
				setRandomWeights();
				weights = getAllWeights();
				
				count_under_tol = 0;
				restart = true;
				error = MSError( testGenerator );
				calcGradients( testGenerator, gradient );
				for ( i = 0; i < numWeights; ++i )
					p[i] = r[i] = -gradient[i];
			}
			
			// Do next iteration.
			++k;
			++iter;
			
		} while ( error > tol2 && iter <= maxIterations );
		
		// Make sure last iteration is saved.
		setAllWeights( weights );
		
		return iter;
	}

	/**
	*  Calculates the network gradient vector dE/dw.  This vector
	*  contains the gradients of the the network mean squared error
	*  with respect to changes in every weight (connection strength)
	*  in the network.
	*
	*  @param  testGenerator  Reference to a class that can provide
	*          training instances.
	*  @param  dEdw  Vector storage space that will be filled in with
	*          the gradient values.
	*  @return No return value.
	**/
	protected void calcGradients( TrInstGenerator testGenerator, float[] dEdw ) {
		
		// Looping variables.
		int i, j;
		
		// The number of network outputs.
		int numOutputs = outputs.length;
		
		// The number of network hidden layers.
		int numHLayers = hLayers.length;
		
		// The number neurons per hiden layer.
		int numNpHL;
		
		// The number of training patterns.
		int Np = testGenerator.numberInstances();
		
		// The total number of weights and biases in this network.
		int numWeights = dEdw.length;
		
		// Position in the Epr vector.
		int pos;
		
		// Zero any gradients left over from an earlier pass.
		for ( i = 0; i < numWeights; ++i )
			dEdw[i] = 0;
		
		// Loop over all the training instances.
		for ( int n = 0; n < Np; ++n ) {
			
			// First clear all the current error values stored in the neurons.
			for ( i = 0; i < numHLayers; ++i ) {
				numNpHL = hLayers[i].length;
				for ( j = 0; j < numNpHL; ++j )
					hLayers[i][j].clearError();
			}
			for ( i = 0; i < inputs.length; ++i )
				inputs[i].clearError();
			
			// Get a training instance and calculate the error at the outputs.
			testGenerator.nextInstance();
			float[] tInputs = testGenerator.getTestInputs();
			float[] tOutputs = testGenerator.getTestOutputs();
			setInputs( tInputs );
			
			// Calculate the error signal at each output node and propogate the error
			// from the output nodes to the last hidden layer.
			// Also, caculate the gradient of each weight that inputs to the output layer.
			// Start with the last input to the last output neuron.
			pos = numWeights - 1;
			for ( i = numOutputs - 1; i >= 0; --i ) {
				float value, error;
				value = outputs[i].getOutput();
				error = tOutputs[i] - value;
				outputs[i].setError( error );
				
				// Calculate the effect of each connection to this neuron on the mean squared error.
				pos = neuronGradients( outputs[i], dEdw, pos );
			}
			
			// Calc error at the nodes in each hidden layer.
			// Also, calculate the gradient of each weight that inputs to a hidden layer.
			for ( i = numHLayers - 1; i >= 0; --i ) {
				numNpHL = hLayers[i].length;
				for ( j = numNpHL - 1; j >= 0; --j )
					// Calculate the effect of each connection to this neuron on the mean squared error.
					pos = neuronGradients( hLayers[i][j], dEdw, pos );
			}
		}	// Next training case
		
		// Normalize the gradients.
		float factor = -2F/Np/numOutputs;
		for ( i = 0; i < numWeights; ++i )
			dEdw[i] *= factor;
	}

	/**
	*  Determine the effect of the input connections to a particular
	*  neuron on the output of the network:  dE/dw
	*  Uses the array dEdw to store the results.  This is called
	*  from calcGradients().
	*
	*  @param  theNeuron  Neuron to have the input weight gradients
	*          calculated for.
	*  @param  dEdw  Vector of gradients of each weight with respect
	*          to the total error of the network for a particular
	*          training pattern.
	*  @param  pos   Position in the dEprdw array for the last weight in
	*          the vector of weighted inputs that feed into this neuron.
	*  @return The current position in the dEpdw vector (position
	*          of 1st input weight -1).
	**/
	protected static int neuronGradients( Neuron theNeuron, float[] dEdw, int pos ) {
		Neuron[] inputNeurons = theNeuron.getInputNeurons();
		
		// If no inputs, the neuron is probably a bias node, just return.
		if ( inputNeurons == null )
			return pos;
		
		// Determine error associated with this neuron:
		float eps = theNeuron.getError();
		
		// Delta = eps * dr/dq
		float delta = eps*theNeuron.getSlope();
		
		// Loop over all the input nodes (last to 1st) passing errors on
		// to them proportional to their contribution.
		for ( int i = inputNeurons.length - 1; i >= 0; --i ) {
			float weight = theNeuron.getWeight( i );
			float input = inputNeurons[i].getOutput();
			dEdw[pos--] += input*delta;
			inputNeurons[i].addErrorIncrement( delta*weight );
		}

		// Return the current position in the dEpdw vector.
		return pos;
	}

	/**
	*  Calculate the magnitude of a vector.
	*
	*  @param  vector  The vector to calculate the magnitude for.
	*  @return The magnitude (length) of the vector.
	**/
	protected static final float vectorMagnitude( float[] vector ) {
		float magnitude = (float)Math.sqrt( vectorProduct( vector, vector ) );
		return magnitude;
	}

	/**
	*  Calculate the product of two vectors (a scalar value).
	*
	*  @param  a  First vector to multiply.
	*  @param  b  Second vector to multiply.
	*  @return The product of the two vectors (a scalar value).
	**/
	protected static final float vectorProduct( float[] a, float[] b ) {
		int length = a.length;
		float value = 0;
		
		for ( int i = 0; i < length; ++i )
			value += a[i]*b[i];
		
		return value;
	}

	/**
	*  Completely copy one array into another.
	*
	*  @param  src Source array.
	*  @param  dst Destination array.
	**/
	protected static final void arrayCopy( float[] src, float[] dst ) {
		System.arraycopy( src, 0, dst, 0, src.length );
	}


}


