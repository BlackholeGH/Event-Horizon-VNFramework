import array
import random
import sys
import math
import os

#Import data science libraries
import numpy as np
import keras
import tensorflow as tf

import logging

#Set tensorflow modes
from tensorflow.python.ops.gradients_util import _NonEagerInputs
logging.getLogger('tensorflow').setLevel(logging.ERROR)

asciiChars = 'ABCDEFX' #Define available char characters for the memory box
asciiR = np.array(list(map(str, list(asciiChars)))) #Numpy array of viable ascii chars
print(asciiR)
print(len(asciiR))

world_dim = 9 #Dimensions of the neural model world sensing input
embed_dim = 2 #Number of dimensions to use for text embeddings
memorybox_sequence_length = 16 #Char sequence length for the memory box
attention_heads = 1 #Number of heads for the IMemBoT multi-head attention layer
movement_neurons = 4 #Number of output neurons to control simulation bots

stored_best_decay_buffer = 50 #Weighting of the original stored fitness against the current fitness for best stored agent decay
allow_stored_decay = True #Boolean for whether best stored agents should have their fitness decay based on recent performance
best_ever_stored_count = 3 #Number of best ever agents to store
best_ever_fitness = [-sys.float_info.max] * best_ever_stored_count #List of the fitnesses of the best ever agents
best_ever_brain = [None] * best_ever_stored_count #Neural control models for the best ever agents

#Function to crossbreed one brain with another
#brain_one: First parent brain
#brain_two: Second parent brain
#proportion_one_to_two: The proportion of genes to be selected from parent one over parent two
#rand_proportion: The chance of genes to be randomly re-initialized
#param_uncertainty: Uncertainty factor on cloned parameters
def do_one_crossbreed(brain_one, brain_two, proportion_one_to_two, rand_proportion, param_uncertainty):
    if(not isinstance(brain_two, type(brain_one))): #Check that the brains have matching architectures
        return None
    else:
        brain_child = type(brain_one)(-1) #Create the child brain
        model_one_weights = brain_one.model.get_weights() #Retrieve parent weights
        model_two_weights = brain_two.model.get_weights()
        child_weights = brain_child.model.get_weights() #Retrieve random child weights
        layerindex = 0
        if not isinstance(brain_one, ChimericMemBoT): #Check that the interbreed is not a Chimeric bot
            for layer in child_weights: #For each child weight layer
                for index, weight in np.ndenumerate(layer): #For each layer
                    if random.random() <= rand_proportion: #Check for random trigger
                        layer[index] = random.uniform(brain_child.initmin, brain_child.initmax) #Randomly re-initialize weight
                    else:
                        if random.random() <= proportion_one_to_two: #Roll to decide if the weight is taken from parent one or two
                            layer[index] = model_one_weights[layerindex][index] + random.gauss(0, param_uncertainty) #Apply parent one weight plus the uncertainty
                        else:
                            layer[index] = model_two_weights[layerindex][index] + random.gauss(0, param_uncertainty) #Apply parent two weight plus the uncertainty
                layerindex += 1 #Increment layer count
            brain_child.model.set_weights(child_weights) #Set child weights to the newly inherited weights
            return brain_child #Return the child
        else: #In the cast of a chimeric bot:
            this_rand = rand_proportion
            if rand_proportion < 0.2: #There is a floor on the allowed randomness for chimeric bots
                this_rand += 0.2
            names = []
            for real_layer in brain_child.model.layers: #search through each layer
                i = 1
                for weights in real_layer.get_weights():
                    names.append(real_layer.name + ("_biases" if i == 2 else "")) #record the layer name for each weight
                    i += 1
            for layer in child_weights: #For each layer in the child weights
                name = names[layerindex] #Retrieve the layer name
                if(name == "x1"):
                    layer[index] = model_one_weights[layerindex][index] #Layer x1 weights are cloned exactly, as these are pre-trained weights from another model
                    layerindex += 1
                elif(name == "forMovement"): #Layer forMovement weights are partially cloned, partially bred
                    for index, weight in np.ndenumerate(layer):
                        if(index[0] > 8): #The index is checked to ensure that the modifiable section is being modified
                            if random.random() <= this_rand: #Check for random reinitialization
                                layer[index] = model_one_weights[layerindex][index] + random.gauss(0, 0.5)
                            else:
                                if random.random() <= proportion_one_to_two: #Decide which parent to inherit from
                                    layer[index] = model_one_weights[layerindex][index] + random.gauss(0, param_uncertainty) #Apply parent one weight plus the uncertainty
                                else:
                                    layer[index] = model_two_weights[layerindex][index] + random.gauss(0, param_uncertainty) #Apply parent two weight plus the uncertainty
                        else:
                            layer[index] = model_one_weights[layerindex][index] #The static portion of these weights are cloned exactly
                    layerindex += 1 #Increment layer count
                else: #For all other layers, the crossbreed operation happens as normal
                    for index, weight in np.ndenumerate(layer):
                        if random.random() <= this_rand: #Check for random trigger
                            layer[index] = random.uniform(brain_child.initmin, brain_child.initmax) #Randomly re-initialize weight
                        else:
                            if random.random() <= proportion_one_to_two: #Roll to decide if the weight is taken from parent one or two
                                layer[index] = model_one_weights[layerindex][index] + random.gauss(0, param_uncertainty) #Apply parent one weight plus the uncertainty
                            else:
                                layer[index] = model_two_weights[layerindex][index] + random.gauss(0, param_uncertainty) #Apply parent two weight plus the uncertainty
                    layerindex += 1
            brain_child.model.set_weights(child_weights) #Set the weights of the chimeric child
            return brain_child #return the child brain

#Function to perform the full population interbreed operation for the genetic algorithm
#brains: The list of brains (neural controllers)
#rand_proportion: Proportion of weights to be randomly reinitialized
#param_uncertainty: Added uncertainty of inherited parameters
def interbreed_by_fitness(brains, rand_proportion, param_uncertainty):
    #Required global variables are accessed
    global best_ever_fitness
    global best_ever_brain
    global best_ever_stored_count
    canonical_brain_type = type(brains[0]) #The type of the brains being bred is stored
    active_socket_ids = [] #Create a list for the active socket IDs for the current generation
    for brain in brains: #For each current generation brain
        active_socket_ids.append(brain.socketID) #Record which socket connection IDs are in use
        if not isinstance(brain, canonical_brain_type): #Check that all brains are of the right type
            return None #Return none if there is a rogue brain of the wrong type
        if allow_stored_decay and brain.best >= 0: #If best ever fitness decay is allowed
            best_ever_fitness[brain.best] = ((best_ever_fitness[brain.best] * (stored_best_decay_buffer - 1)) + brain.fitness) / stored_best_decay_buffer #Decay the stored fitness of the best ever brains via a weighted calculation based on their current performance
    best_ever_fitness.sort(key=lambda fitness: fitness, reverse=True) #Re-sort the stored brain fitnesses
    best_ever_brain.sort(key=lambda brain: brain.fitness if brain != None else 0, reverse=True) #Re-sort the stored brains in order of fitnesses
    sorted_brains = sorted(brains, key=lambda brain: brain.fitness, reverse=True) #Now the entire current generation is sorted by fitness
    surviving_brains = sorted_brains[0:math.floor(len(sorted_brains) / 2)] #Remove the least fit half of the current generation
    parent_brains = surviving_brains.copy() #Copy the surviving half of the brains
    surviving_brains = [] #Clear the surviving brain list
    remerge_brains = [] #Create the "remerge" list of additional brains to include in the next generation
    for i in range(0, best_ever_stored_count): #For the number of best stored brains
        parent_brain = parent_brains[i]  #Check the best parent brains from the current generation
        for j in range(0, best_ever_stored_count): #For each of the best stored brains
            if(parent_brain.fitness > best_ever_fitness[j]): #Compare the fitness of the best brains of this generation to the fitness of the best stored
                parent_brain.best = j #If the parent brain is better set it to be one of the best ever stored
                best_ever_fitness.insert(j, parent_brain.fitness) #Insert new best brain fitness
                best_ever_brain.insert(j, parent_brain) #Insert new best brain model
                print(f"New no. {j + 1} all-time brain: socket ID {parent_brain.socketID} with a fitness of {parent_brain.fitness}.")
                break
    if(len(best_ever_brain) > best_ever_stored_count): #If there are too many best ever brains stored
        for i in range(best_ever_stored_count - 1, len(best_ever_brain)): #Go through the worst stored brains
            if best_ever_brain[i] != None: #Check that the brain exists
                best_ever_brain[i].best = -1 #Set its internal record of its best ever placement to -1
    best_ever_brain = best_ever_brain[0:best_ever_stored_count] #Clip the end off of the best ever brain list
    best_ever_fitness = best_ever_fitness[0:best_ever_stored_count] #Clip the end off of the fitness list
    for i in range(0, best_ever_stored_count): #Now for the final best ever brains list
        if(best_ever_brain[i] != None): #Check that the brain exists
            best_ever_brain[i].best = i #Record the placement of the brain in the list internally
            remerge_brains.append(best_ever_brain[i].copy()) #Re-deploy the brain for the next generation
    for brain in parent_brains: #For each parent brain
        print(brain.fitness)
        rank = parent_brains.index(brain) + 1 #Parent brains are ranked by fitness
        if len(remerge_brains) < len(parent_brains): #If there is still room in the list of additional brains to add into the next generation
            #max_reinstantiate = math.floor(len(parent_brains) / 4)
            max_reinstantiate = 3 #Set the limit for cloning best performing brains from the previous generation
            for i in range(0, max((int)((max_reinstantiate) - (rank - 1)), 1)): #Clone the brains multiple times, reducing the number by one for each rank
                remerge_brains.append(brain.copy())
            while len(remerge_brains) > len(parent_brains): #If too many brains have been added, remove them
                remerge_brains.pop(-1)
        #spouse_index = math.floor(random.triangular(0, len(parent_brains), 0)) #Triangular spouse selection model
        spouse_index = 0 #Harem mating spouse selection model
        spouse = sorted_brains[spouse_index] #Select spouse by index
        avr_parent_fitness = float((brain.fitness + spouse.fitness) / 2) #Calculate average parent fitness
        #adjusted_rand = min(1, float(1 - ((avr_parent_fitness + 50000) / 80000))) * rand_proportion #Adjusted rand can increase randomness probability for worse performing brains
        adjusted_rand = 0.02 #Alternatively the random level can be set to a flag level
        proportion_one_to_two = (spouse_index + 1) / (rank + spouse_index + 1) #Gene selection is performed in proportion to each parent's fitness ranking
        child = do_one_crossbreed(brain, spouse, proportion_one_to_two, adjusted_rand, param_uncertainty) #The child brain is bred
        surviving_brains.append(child) #The child brain is added to the list of next generation brains
    surviving_brains.extend(remerge_brains) #The "remerge" list is added to the next generation
    while len(surviving_brains) < len(sorted_brains): #If the new generation list is somehow too short
        surviving_brains.append(canonical_brain_type(-1)) #Add a randomly initialized brain to pad it out
    index = 0
    surviving_brains.sort(key=lambda brain: random.random()) #The next generation is randomly shuffled
    for new_generation_brain in surviving_brains: #For each of the next generation brains
        new_generation_brain.socketID = active_socket_ids[index] #Assign it one of the active socket IDs
        index += 1
    return surviving_brains #Return the new generation

#The IOHandler class defines a handler for a socket connection, which may or may not include the processing of a neural model
class IOHandler:
    def __init__(self, socketID):
        self.socketID = socketID #ID of the socket connection being handled
        self.nextOutput = None #The next output to be dispatched over the socket
        return
    def get_output(self): #Function to return the next output to be sent
        return self.nextOutput

#SystemHandler is a special case of IOHandler that does not contain a neural model but instead processes system commands send over the socket bridge
class SystemHandler(IOHandler):
    simcode = str(1) #Initialize the current SimCode
    runsim = False #Initialize the boolean for whether or not the simulation is running
    do_interbreed = False #Initialize the boolean for whether the interbeed operation is active
    interbreed_rand_proportion = 0.1 #Initialize the randomization proportion
    interbreed_param_uncertainty = 0.05 #Initialize the parameter uncertainty
    apply_next_generation = False #Initialize the boolean for whether or not the next genertation needs to be applied
    def __init__(self, socketID):
        super(SystemHandler, self).__init__(socketID) #Run the parent constructor
    #handle_input handles incoming data over the socket
    #doubles: the data sent over the socket as a list of double values
    #iohandlerlist: the full list of IO handlers
    def handle_input(self, doubles, iohandlerlist):
        i = 0
        while i < len(doubles): #Iterate through each received value
            match doubles[i]: #Match certain double values, which can be parsed as specific system commands
                case float(1): #Value 1: set the simcode
                    SystemHandler.simcode = doubles[i + 1] #The simcode is set
                    print(f"Simulation code set to {SystemHandler.simcode}")
                    i = i + 1
                case float(2): #Value 2: save brains to file under the current simcode
                    print(f"Saving weights within working directory {os.getcwd()}.")
                    for io in iohandlerlist: #For each Brain in the handler list
                        if isinstance(io, Brain):
                            io.save_weights() #Save the weights of that brain
                    print(f"Saved simulation {SystemHandler.simcode} weights to file.")
                case float(3): #Value 3: Load brain weights from file by the current simcode
                    print(f"Loading weights within working directory {os.getcwd()}.")
                    for io in iohandlerlist: #For each IO handler in the list
                        if isinstance(io, Brain): #If it is a brain
                            io.load_weights(int(doubles[i + 1]), io.socketID) #Load the saved weights to that brain
                    print(f"Loaded simulation {int(doubles[i + 1])} weights to current brains.")
                    i = i + 1
                case float(4): #Value 4: Set the simulation run state
                    SystemHandler.runsim = (doubles[i + 1] == 1) #Set the boolean value to the specified parameter value
                    print(f"Simulation running? Set to {str(SystemHandler.runsim)}")
                    i = i + 1
                case float(5): #Value 5: Perform a neural model interbreed operation
                    SystemHandler.interbreed_rand_proportion = doubles[i + 1] #The rand proportion is set
                    SystemHandler.interbreed_param_uncertainty = doubles[i + 2] #The parameter uncertainty is set
                    print(f"Interbreed event requested! Generating next generation based on current fitness values, a mutation calculation weighting of {SystemHandler.interbreed_rand_proportion} and a parameter uncertainty of {SystemHandler.interbreed_param_uncertainty}.")
                    SystemHandler.do_interbreed = True #The boolean flag to do the interbreed is set
                    i = i + 2
                case float(6): #Value 6: Apply the queued next generation controllers, handing them active control
                    print(f"Generation advance requested! Reinitializing socket handlers with child generation neural models.")
                    SystemHandler.apply_next_generation = True #The boolean flag for this is set
            i+=1
        return

#Each "Brain" is an IOHandler attached to a socket connection, but also defines a neural model that can be queried to control a given agent
class Brain(IOHandler):
    def __init__(self, socketID): #Initialize the generic brain
        super(Brain, self).__init__(socketID) #Run the parent constructor
        if(not hasattr(self, "initmin")):
            self.initmin = -0.05 #Set the weight initialization minimum
        if(not hasattr(self, "initmax")):
            self.initmax = 0.05 #Set the weight initialization maximum
        self.model = self._setup_model() #Set up this Brain's neural model
        self.fitness = 0 #Initialize this Brain's model fitness
        self.best = -1 #Initialize this Brain's "best ever" ranking to -1
    def _setup_model(self): #The base "Brain" class does not define a neural model. Models are defined by the subclasses
        return None
    def process_system_codes(self, codes): #Brain IOHandlers can also process system commands sent over the socket bridge about them specifically
        i = 1
        while i < len(codes):
            match codes[i]:
                case float(1): #There is only one current system code for a Brain handler, which is to set the neural model fitness value for the genetic algorithm
                    self.fitness = codes[i + 1]
                    i += 1
            i += 1
        self.nextOutput = array.array('d', [sys.float_info.max] * 128) #The system code is replied to by sending an acknowledgement code of all max value doubles
    def save_weights(self): #Function to save weights for this brain
        print(f"Saving weights for Brain model with ID {self.socketID}...")
        self.model.save(f"./PyModelWeights/Simulation_{str(SystemHandler.simcode)}/brain_{self.socketID}", overwrite=True) #Weights are saved by the current sim code and using the socket ID
    def load_weights(self, simcode, socketID): #Function to load weights on file
        print(f"Loading weights to Brain model with ID {socketID}...")
        try:
            self.model = tf.keras.models.load_model(f"./PyModelWeights/Simulation_{str(simcode)}/brain_{socketID}") #Weights are loaded from the current specified sim ID and using the Brain's socket ID
        except:
            try:
                self.model = tf.keras.models.load_model(f"./PyModelWeights/Simulation_{str(simcode)}.0/brain_{socketID}")
            except:
                print(f"Could not load brain {socketID}.")
    def handle_input(self, doubles): #Function to handle incoming data sent over the socket as a list od doubles
        if doubles[0] == sys.float_info.max: #If the received data is a system code
            self.process_system_codes(doubles) #Process as a system code
        else: #If it is standard data
            if SystemHandler.runsim: #Check if the simulation is running
                infer = [1]
                infer.extend(np.array(self.model(np.array([doubles[0:world_dim]]))).tolist()[0]) #Use input data and infer outputs from the neural model
                self.nextOutput = array.array('d', infer) #Dispatch processed model output back over socket
            else:
                self.nextOutput = array.array('d', [float(0)] * 128) #Otherwise send an empty response acknowledgement
    def copy(self): #Function to clone the current brain
        copy_of_self = type(self)(self.socketID) #Clone socket ID
        copy_of_self.model = tf.keras.models.clone_model(self.model) #Clone neural model
        copy_of_self.best = self.best #Clone "best ever" ranking
        return copy_of_self

#The "SimpleFeedForwardDense" class defines the equivalent neural model and extends Brain
class SimpleFeedForwardDense(Brain):
    def __init__(self, socketID): #Set initial parameters
        self.initmin = -1
        self.initmax = 1
        super(SimpleFeedForwardDense, self).__init__(socketID)

    def _setup_model(self): #Define the neural model using the keras API
        trueWorldInputs = tf.keras.Input(shape=(world_dim)) #World sensing inputs

        worldInputs = tf.keras.layers.Dense( #Dense input layer
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="worldInputs")(trueWorldInputs)


        x = tf.keras.layers.Dense( #Hidden layer
            units = world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="x1"
        )(worldInputs)

        #x = tf.keras.layers.Dense( #Optional second hidden layer
        #    units=world_dim,
        #    activation='relu',
        #    kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    name="x2"
        #)(x)

        #x = tf.keras.layers.Dense( #Optional third input layer
        #    units=world_dim,
        #    activation='relu',
        #    kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    name="x3"
        #)(x)

        forMovement = tf.keras.layers.Dense( #Output layer to control agent movement
            units=movement_neurons,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="forMovement")(x)

        fullModel = tf.keras.Model( #The full model is compiled
            inputs=[worldInputs],
            outputs=[forMovement])
        #fullModel.summary()
        weights = fullModel.get_weights()

        #print(len(fullModel.get_weights()))
        #i = 0
        #for layerWeights in weights:
        #    i+=1
        #    print(f"Layer {i}: {len(layerWeights)}")

        return fullModel #Return the model

#This Brain subclass implements the first version of the IMemBoT model
class IterativeMemoryTransformer(Brain):
    def __init__(self, socketID):
        self.initmin = -1 #Initialize the weight init bounds
        self.initmax = 1
        super(IterativeMemoryTransformer, self).__init__(socketID) #Run parent constructor
        self.memory_box = ["X"] * memorybox_sequence_length #Initialize the memory box

    #Bespoke input handling function fot the IMemBoT models
    #doubles: Inputs sent over the socket bridge
    def handle_input(self, doubles):
        if doubles[0] == sys.float_info.max: #Check to see if the input is a system code
            self.process_system_codes(doubles) #Process system code
        else: #If it is standard sensory inout
            if SystemHandler.runsim: #If the simulation is running
                model_run = self.model([np.array([doubles[0:world_dim]]), np.array([''.join(self.memory_box)])]) #Infer from the model
                model_outs = []
                for tensor in model_run:
                    model_outs.append(np.array(tensor).tolist()[0]) #Extract model outputs to list
                infer = [1]
                infer.extend(model_outs[0])
                self.nextOutput = array.array('d', infer) #Set next socket output to the inferred movement commands/output layer
                if(model_outs[3][0] > 0.5): #Action first write cell
                    index = 0
                    largest = 0
                    write_index = -1
                    for prob in model_outs[2]: #Select write index
                        if prob > largest:
                            largest = prob
                            write_index = index
                        index += 1
                    index = 0
                    largest = 0
                    val_index = -1
                    for prob in model_outs[1]: #Select write value
                        if prob > largest:
                            largest = prob
                            val_index = index
                        index += 1
                    self.memory_box[write_index] = asciiChars[val_index] #Update memory box
                if(len(model_outs) > 4 and model_outs[6][0] > 0.5): #Action second write cell
                    index = 0
                    largest = 0
                    write_index = -1
                    for prob in model_outs[5]: #Select write index
                        if prob > largest:
                            largest = prob
                            write_index = index
                        index += 1
                    index = 0
                    largest = 0
                    val_index = -1
                    for prob in model_outs[4]: #Select write value
                        if prob > largest:
                            largest = prob
                            val_index = index
                        index += 1
                    self.memory_box[write_index] = asciiChars[val_index] #Update memory box
                #Additional write cells, if included, would continue this pattern
            else:
                self.nextOutput = array.array('d', [float(0)] * 128) #If simulation is not running, send an empty acknowledgement back over the socket

    def _setup_model(self):
        trueWorldInputs = tf.keras.Input(shape=(world_dim)) #Create world sensing inputs

        worldInputs = tf.keras.layers.Dense( #Dense input layer
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="worldInputs")(trueWorldInputs)

        boxInput = tf.keras.Input( #Memory box text input layer
            shape=(1,),
            dtype=tf.string)

        #print(boxInput.shape)

        textVec = tf.keras.layers.TextVectorization( #Text vectorization layer
            max_tokens=len(asciiR) + 2,
            output_mode='int',
            output_sequence_length=memorybox_sequence_length,
            standardize=None,
            split="character",
            vocabulary=asciiR,
            name="textVec")(boxInput)

        #print(textVec.shape)

        textEmb = tf.keras.layers.Embedding( #Embedding layer
            input_dim=len(asciiR) + 3,
            output_dim=embed_dim,
            embeddings_initializer=tf.keras.initializers.random_normal,
            input_length=memorybox_sequence_length,
            name="textEmb")(textVec)

        expandForReshape = tf.keras.layers.Dense( #Dense expansion layer
            units = world_dim * embed_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="expandForReshape")(worldInputs)

        embedFlat = tf.keras.layers.Reshape((memorybox_sequence_length * embed_dim,))(textEmb) #Flattening layer

        confluence = tf.keras.layers.Dense( #Concatenation and hidden layer
            units=8 * embed_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="confluence")(tf.keras.layers.concatenate([expandForReshape, embedFlat], name="conflConcat"))

        reExpand = tf.keras.layers.Reshape((8, embed_dim), #Reshape layer
                                           name="reExpand")(confluence)

        attend = tf.keras.layers.MultiHeadAttention( #Multi-head attention layer
            num_heads=attention_heads,
            key_dim=int(embed_dim / attention_heads),
            name="attend"
        )(reExpand, reExpand)

        add = tf.keras.layers.Add(name="add")([attend, reExpand]) #Residual addition layer

        norm = tf.keras.layers.LayerNormalization(axis=[1, 2], #Normalization layer
                                                  name="norm")(add)

        x = tf.keras.layers.Dense( #Dense hidden layer 1 for world input
            units = world_dim * embed_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="x1"
        )(expandForReshape)
        x = tf.keras.layers.Dense( #Dense hidden layer 2 for world input
            units=world_dim * embed_dim,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="x2"
        )(x)
        x = tf.keras.layers.Dense( #Dense hidden layer 3 for world input
            units=world_dim * embed_dim,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="x3"
        )(x)

        flatAttn = tf.keras.layers.Flatten(name="flatAttn")(norm) #Flatten the attention stack output

        carry = tf.keras.layers.Dense( #Concatenate the two parts of the model
            units = 12,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="carry"
        )(tf.keras.layers.concatenate([x, flatAttn], name="carryConcat"))

        forMovement = tf.keras.layers.Dense( #Define movement putput
            units=movement_neurons,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="forMovement")(carry)

        writeValueOne = tf.keras.layers.Dense( #Write cell 1 value dense layer
            units=len(asciiChars),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeValueOne")(carry)

        writeValueOutOne = tf.keras.layers.Softmax(name="writeValueOutOne")(writeValueOne) #Write cell 1 value softmax layer

        writePosOne = tf.keras.layers.Dense( #Write cell 1 position dense layer
            units=memorybox_sequence_length,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writePosOne")(carry)

        writePosOutOne = tf.keras.layers.Softmax(name="writePosOutOne")(writePosOne) #Write cell 1 position softmax layer

        writeBoolOne = tf.keras.layers.Dense( #Write cell 1 boolean trigger layer
            units=1,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeBoolOne")(carry)

        writeValueTwo = tf.keras.layers.Dense( #Write cell 2 value dense layer
            units=len(asciiChars),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeValueTwo")(carry)

        writeValueOutTwo = tf.keras.layers.Softmax(name="writeValueOutTwo")(writeValueTwo) #Write cell 2 value softmax layer

        writePosTwo = tf.keras.layers.Dense( #Write cell 2 position softmax layer
            units=memorybox_sequence_length,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writePosTwo")(carry)

        writePosOutTwo = tf.keras.layers.Softmax(name="writePosOutTwo")(writePosTwo) #Write cell 2 position softmax layer

        writeBoolTwo = tf.keras.layers.Dense( #Write cell 2 boolean trigger layer
            units=1,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeBoolTwo")(carry)

        writeValueThree = tf.keras.layers.Dense( #Write cell 3 value softmax layer
            units=94,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writeValueThree")(carry)

        writeValueOutThree = tf.keras.layers.Softmax(name="writeValueOutThree")(writeValueThree) #Write cell 3 value softmax layer

        writePosThree = tf.keras.layers.Dense( #Write cell 3 position dense layer
            units=memorybox_sequence_length,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writePosThree")(carry)

        writePosOutThree = tf.keras.layers.Softmax(name="writePosOutThree")(writePosThree) #Write cell 3 position softmax layer
        
        writeBoolThree = tf.keras.layers.Dense( #Write cell 3 boolean trigger layer
            units=1,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writeBoolThree")(carry)

        fullModel = tf.keras.Model( #Compile full model
            inputs=[worldInputs, boxInput],
            outputs=[forMovement,
                     writeValueOutOne, writePosOutOne, writeBoolOne,
                     writeValueOutTwo, writePosOutTwo, writeBoolTwo])
        #fullModel.summary()
        #weights = fullModel.get_weights()

        #print(len(fullModel.get_weights()))
        #for layerWeights in weights:
        #    print(len(layerWeights))

        return fullModel

#The second IMemBoT model is similar to the first, but attempts to condense down the model architecture. It inherits from the first version but defines its own model structure
class IMemBoT2(IterativeMemoryTransformer):
    def __init__(self, socketID):
        super(IMemBoT2, self).__init__(socketID) #Run parent constructor

    def _setup_model(self):
        trueWorldInputs = tf.keras.Input(shape=(world_dim)) #Get world sensing inputs

        worldInputs = tf.keras.layers.Dense( #World sensing dense input layer
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="worldInputs")(trueWorldInputs)


        #x = tf.keras.layers.Dense(
        #    units = world_dim,
        #    activation='relu',
        #    kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    name="x1"
        #)(worldInputs)

        boxInput = tf.keras.Input( #Memory box input layer
            shape=(1,),
            dtype=tf.string)

        #print(boxInput.shape)

        textVec = tf.keras.layers.TextVectorization( #Text vectorization layer
            max_tokens=len(asciiR) + 2,
            output_mode='int',
            output_sequence_length=memorybox_sequence_length,
            standardize=None,
            split="character",
            vocabulary=asciiR,
            name="textVec")(boxInput)

        #print(textVec.shape)

        textEmb = tf.keras.layers.Embedding( #Text embedding layer
            input_dim=len(asciiR) + 3,
            output_dim=embed_dim,
            embeddings_initializer=tf.keras.initializers.random_normal,
            input_length=memorybox_sequence_length,
            name="textEmb")(textVec)

        attend = tf.keras.layers.MultiHeadAttention( #Multi-head attention layer
            num_heads=attention_heads,
            key_dim=int(embed_dim / attention_heads),
            name="attend"
        )(textEmb, textEmb)

        add = tf.keras.layers.Add(name="add")([attend, textEmb]) #Residual addition layer

        norm = tf.keras.layers.LayerNormalization(axis=[1, 2], #Normalization layer
                                                  name="norm")(add)

        flatAttn = tf.keras.layers.Flatten(name="flatAttn")(norm) #Flatten attention stack layer

        carry = tf.keras.layers.Dense( #Concatenate both halves of model
            units = 8,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="carry"
        )(tf.keras.layers.concatenate([worldInputs, flatAttn], name="carryConcat"))

        forMovement = tf.keras.layers.Dense( #Movement output layer
            units=movement_neurons,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="forMovement")(carry)

        writeValueOne = tf.keras.layers.Dense( #Write cell 1 value dense layer
            units=len(asciiChars),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeValueOne")(carry)

        writeValueOutOne = tf.keras.layers.Softmax(name="writeValueOutOne")(writeValueOne) #Write cell 1 value softmax layer

        writePosOne = tf.keras.layers.Dense( #Write cell 1 position dense layer
            units=memorybox_sequence_length,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writePosOne")(carry)

        writePosOutOne = tf.keras.layers.Softmax(name="writePosOutOne")(writePosOne) #Write cell 1 position softmax layer

        writeBoolOne = tf.keras.layers.Dense( #Write cell 1 boolean trigger neuron
            units=1,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeBoolOne")(carry)

        fullModel = tf.keras.Model( #Compile full model
            inputs=[worldInputs, boxInput],
            outputs=[forMovement,
                     writeValueOutOne, writePosOutOne, writeBoolOne])
        #fullModel.summary()
        #weights = fullModel.get_weights()

        #print(len(fullModel.get_weights()))
        #for layerWeights in weights:
        #    print(len(layerWeights))

        return fullModel

base_model = tf.keras.models.load_model(f"./PyModelWeights/Simulation_1/brain_4") #Load pre-trained weights

#The Chimeric model is similar to the IMemBoT2 model but loads some pre-trained weights from a dense model
class ChimericMemBoT(IterativeMemoryTransformer):
    def __init__(self, socketID):
        super(ChimericMemBoT, self).__init__(socketID) #Run parent constructor

    def _setup_model(self):

        #prime with pretrained

        #print(f"Loading weights to Brain model with ID {self.socketID}...")

        trueWorldInputs = tf.keras.Input(shape=(world_dim)) #Get world sensing inputs

        worldInputs = tf.keras.layers.Dense( #Dense input layer
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="worldInputs")(trueWorldInputs)

        x = tf.keras.layers.Dense( #Single hidden layer
            units = world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="x1"
        )(worldInputs)

        boxInput = tf.keras.Input( #Memory box input layer
            shape=(1,),
            dtype=tf.string)

        #print(boxInput.shape)

        textVec = tf.keras.layers.TextVectorization( #Text vectorization layer
            max_tokens=len(asciiR) + 2,
            output_mode='int',
            output_sequence_length=memorybox_sequence_length,
            standardize=None,
            split="character",
            vocabulary=asciiR,
            name="textVec")(boxInput)

        #print(textVec.shape)

        textEmb = tf.keras.layers.Embedding( #Text embedding layer
            input_dim=len(asciiR) + 3,
            output_dim=embed_dim,
            embeddings_initializer=tf.keras.initializers.random_normal,
            input_length=memorybox_sequence_length,
            name="textEmb")(textVec)

        attend = tf.keras.layers.MultiHeadAttention( #Multi-head attention layer
            num_heads=attention_heads,
            key_dim=int(embed_dim / attention_heads),
            name="attend"
        )(textEmb, textEmb)

        add = tf.keras.layers.Add(name="add")([attend, textEmb]) #Residual addition layer

        norm = tf.keras.layers.LayerNormalization(axis=[1, 2], #Normalization layer
                                                  name="norm")(add)

        flatAttn = tf.keras.layers.Flatten(name="flatAttn")(norm) #Flatten attention stack layer

        attention_extend = len(flatAttn.shape)

        prederive = tf.keras.layers.concatenate([x, flatAttn], name="prederive") #Concatenate both model halves

        forMovement = tf.keras.layers.Dense( #Movement output layer
            units=movement_neurons,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.zeros(),
            bias_initializer=tf.keras.initializers.zeros(),
            name="forMovement")(prederive)

        writeValueOne = tf.keras.layers.Dense( #Write cell 1 value dense layer
            units=len(asciiChars),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeValueOne")(prederive)

        writeValueOutOne = tf.keras.layers.Softmax(name="writeValueOutOne")(writeValueOne) #Write cell 1 value softmax layer

        writePosOne = tf.keras.layers.Dense( #Write cell 1 position dense layer
            units=memorybox_sequence_length,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writePosOne")(prederive)

        writePosOutOne = tf.keras.layers.Softmax(name="writePosOutOne")(writePosOne) #Write cell 1 position softmax layer

        writeBoolOne = tf.keras.layers.Dense( #Write cell 1 boolean trigger neuron
            units=1,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeBoolOne")(prederive)

        fullModel = tf.keras.Model(  #Compile full model
            inputs=[worldInputs, boxInput],
            outputs=[forMovement,
                     writeValueOutOne, writePosOutOne, writeBoolOne])

        base_weights = base_model.get_weights() #Get pre-trained weights
        layers = fullModel.layers #Get current model weights
        for layer in layers:
            match layer.name:
                case "x1": #Apply pre-trained hidden layer weights
                    layer.set_weights([base_weights[0], base_weights[1]])
                case "forMovement": #Apply pre-trained output layer weights
                    mov_weights = [base_weights[2], base_weights[3]]
                    extension = ([[0, 0, 0, 0]] * attention_extend)
                    true_weights_list = mov_weights[0].tolist()
                    true_weights_list.extend(extension)
                    mov_weights[0] = np.array(true_weights_list)
                    layer.set_weights(mov_weights)
                    got_weights = layer.get_weights()
        #fullModel.summary()
        #weights = fullModel.get_weights()

        #print(len(fullModel.get_weights()))
        #for layerWeights in weights:
        #    print(len(layerWeights))

        return fullModel

#imembot = IterativeMemoryTransformer(0)
#sffd = SimpleFeedForwardDense(0)
#chimem = ChimericMemBoT(1)
#imem2 = IMemBoT2(0)