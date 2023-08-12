import array
import random
import sys
import math
import os

import numpy as np
import keras
import tensorflow as tf

import logging

from tensorflow.python.ops.gradients_util import _NonEagerInputs
logging.getLogger('tensorflow').setLevel(logging.ERROR)

asciiChars = 'ABCDEFX'
asciiR = np.array(list(map(str, list(asciiChars))))
print(asciiR)
print(len(asciiR))

world_dim = 9
embed_dim = 2
memorybox_sequence_length = 16
attention_heads = 1
movement_neurons = 4

stored_best_decay_buffer = 50 #prev was 50
allow_stored_decay = True
best_ever_stored_count = 3
best_ever_fitness = [-sys.float_info.max] * best_ever_stored_count
best_ever_brain = [None] * best_ever_stored_count

def do_one_crossbreed(brain_one, brain_two, proportion_one_to_two, rand_proportion, param_uncertainty):
    if(not isinstance(brain_two, type(brain_one))):
        return None
    else:
        brain_child = type(brain_one)(-1)
        model_one_weights = brain_one.model.get_weights()
        model_two_weights = brain_two.model.get_weights()
        child_weights = brain_child.model.get_weights()
        layerindex = 0
        if not isinstance(brain_one, ChimericMemBoT):
        #if True:
            for layer in child_weights:
                for index, weight in np.ndenumerate(layer):
                    if random.random() <= rand_proportion:
                        layer[index] = random.uniform(brain_child.initmin, brain_child.initmax)
                        print("Random triggered!")
                    else:
                        if random.random() <= proportion_one_to_two:
                            layer[index] = model_one_weights[layerindex][index] + random.gauss(0, param_uncertainty)
                            #print(model_one_weights[layerindex][index])
                        else:
                            layer[index] = model_two_weights[layerindex][index] + random.gauss(0, param_uncertainty)
                            #print(model_two_weights[layerindex][index])
                        #print(layer[index])
                layerindex += 1
            brain_child.model.set_weights(child_weights)
            return brain_child
        else:
            this_rand = rand_proportion
            if rand_proportion < 0.2:
                this_rand += 0.2
            names = []
            for real_layer in brain_child.model.layers:
                i = 1
                for weights in real_layer.get_weights():
                    names.append(real_layer.name + ("_biases" if i == 2 else ""))
                    i += 1
            for layer in child_weights:
                #print(len(layer))
                name = names[layerindex]
                #print(name)
                if(name == "x1"):
                    layer[index] = model_one_weights[layerindex][index]
                    layerindex += 1
                elif(name == "forMovement"):
                    for index, weight in np.ndenumerate(layer):
                        if(index[0] > 8):
                            if random.random() <= this_rand:
                                layer[index] = model_one_weights[layerindex][index] + random.gauss(0, 0.5)
                                #print("Random triggered!")
                            else:
                                if random.random() <= proportion_one_to_two:
                                    layer[index] = model_one_weights[layerindex][index] + random.gauss(0, param_uncertainty)
                                else:
                                    layer[index] = model_two_weights[layerindex][index] + random.gauss(0, param_uncertainty)
                        else:
                            layer[index] = model_one_weights[layerindex][index]
                    layerindex += 1
                else:
                    for index, weight in np.ndenumerate(layer):
                        if random.random() <= this_rand:
                            layer[index] = random.uniform(brain_child.initmin, brain_child.initmax)
                            #print("Random triggered!")
                        else:
                            if random.random() <= proportion_one_to_two:
                                layer[index] = model_one_weights[layerindex][index] + random.gauss(0, param_uncertainty)
                            else:
                                layer[index] = model_two_weights[layerindex][index] + random.gauss(0, param_uncertainty)
                    layerindex += 1
            brain_child.model.set_weights(child_weights)
            return brain_child

def interbreed_by_fitness(brains, rand_proportion, param_uncertainty):
    global best_ever_fitness
    global best_ever_brain
    global best_ever_stored_count
    canonical_brain_type = type(brains[0])
    active_socket_ids = []
    for brain in brains:
        active_socket_ids.append(brain.socketID)
        if not isinstance(brain, canonical_brain_type):
            return None
        if allow_stored_decay and brain.best >= 0:
            best_ever_fitness[brain.best] = ((best_ever_fitness[brain.best] * (stored_best_decay_buffer - 1)) + brain.fitness) / stored_best_decay_buffer
    best_ever_fitness.sort(key=lambda fitness: fitness, reverse=True)
    best_ever_brain.sort(key=lambda brain: brain.fitness if brain != None else 0, reverse=True)
    sorted_brains = sorted(brains, key=lambda brain: brain.fitness, reverse=True)
    surviving_brains = sorted_brains[0:math.floor(len(sorted_brains) / 2)]
    parent_brains = surviving_brains.copy()
    surviving_brains = []
    remerge_brains = []
    for i in range(0, best_ever_stored_count):
        parent_brain = parent_brains[i]
        for j in range(0, best_ever_stored_count):
            if(parent_brain.fitness > best_ever_fitness[j]):
                parent_brain.best = j
                best_ever_fitness.insert(j, parent_brain.fitness)
                best_ever_brain.insert(j, parent_brain)
                print(f"New no. {j + 1} all-time brain: socket ID {parent_brain.socketID} with a fitness of {parent_brain.fitness}.")
                break
    if(len(best_ever_brain) > best_ever_stored_count):
        for i in range(best_ever_stored_count - 1, len(best_ever_brain)):
            if best_ever_brain[i] != None:
                best_ever_brain[i].best = -1
    best_ever_brain = best_ever_brain[0:best_ever_stored_count]
    best_ever_fitness = best_ever_fitness[0:best_ever_stored_count]
    for i in range(0, best_ever_stored_count):
        if(best_ever_brain[i] != None):
            best_ever_brain[i].best = i
            remerge_brains.append(best_ever_brain[i].copy())
    for brain in parent_brains:
        print(brain.fitness)
        rank = parent_brains.index(brain) + 1
        if len(remerge_brains) < len(parent_brains):
            #max_reinstantiate = math.floor(len(parent_brains) / 4)
            max_reinstantiate = 3
            for i in range(0, max((int)((max_reinstantiate) - (rank - 1)), 1)):
                remerge_brains.append(brain.copy())
            while len(remerge_brains) > len(parent_brains):
                remerge_brains.pop(-1)
        #spouse_index = math.floor(random.triangular(0, len(parent_brains), 0))
        spouse_index = 0
        spouse = sorted_brains[spouse_index]
        avr_parent_fitness = float((brain.fitness + spouse.fitness) / 2)
        #adjusted_rand = min(1, float(1 - ((avr_parent_fitness + 50000) / 80000))) * rand_proportion
        adjusted_rand = 0.02
        proportion_one_to_two = (spouse_index + 1) / (rank + spouse_index + 1)
        #print(f"Rank 1: {rank}, Rank 2: {spouse_index + 1}, Proportion 1:2: {proportion_one_to_two}")
        child = do_one_crossbreed(brain, spouse, proportion_one_to_two, adjusted_rand, param_uncertainty)
        surviving_brains.append(child)
    surviving_brains.extend(remerge_brains)
    while len(surviving_brains) < len(sorted_brains):
        surviving_brains.append(canonical_brain_type(-1))
    index = 0
    surviving_brains.sort(key=lambda brain: random.random())
    for new_generation_brain in surviving_brains:
        new_generation_brain.socketID = active_socket_ids[index]
        index += 1
    #index = 0
    #for new_generation_brain in surviving_brains:
    #    index += 1
    #    print(f"Brain {index}: Socket {new_generation_brain.socketID}")
    return surviving_brains

class IOHandler:
    def __init__(self, socketID):
        self.socketID = socketID
        self.nextOutput = None
        return
    def get_output(self):
        return self.nextOutput

class SystemHandler(IOHandler):
    simcode = str(1)
    runsim = False
    do_interbreed = False
    interbreed_rand_proportion = 0.1
    interbreed_param_uncertainty = 0.05
    apply_next_generation = False
    def __init__(self, socketID):
        super(SystemHandler, self).__init__(socketID)
    def handle_input(self, doubles, iohandlerlist):
        i = 0
        while i < len(doubles):
            match doubles[i]:
                case float(1): #set_simcode
                    SystemHandler.simcode = doubles[i + 1]
                    print(f"Simulation code set to {SystemHandler.simcode}")
                    i = i + 1
                case float(2): #save_brains_to_current_simcode
                    print(f"Saving weights within working directory {os.getcwd()}.")
                    for io in iohandlerlist:
                        if isinstance(io, Brain):
                            io.save_weights()
                    print(f"Saved simulation {SystemHandler.simcode} weights to file.")
                case float(3): #load_sim_to_brains_by_code
                    print(f"Loading weights within working directory {os.getcwd()}.")
                    for io in iohandlerlist:
                        if isinstance(io, Brain):
                            io.load_weights(int(doubles[i + 1]), io.socketID)
                    print(f"Loaded simulation {int(doubles[i + 1])} weights to current brains.")
                    i = i + 1
                case float(4): #set_sim_running_state
                    SystemHandler.runsim = (doubles[i + 1] == 1)
                    print(f"Simulation running? Set to {str(SystemHandler.runsim)}")
                    i = i + 1
                case float(5): #do_interbreed
                    SystemHandler.interbreed_rand_proportion = doubles[i + 1]
                    SystemHandler.interbreed_param_uncertainty = doubles[i + 2]
                    print(f"Interbreed event requested! Generating next generation based on current fitness values, a mutation calculation weighting of {SystemHandler.interbreed_rand_proportion} and a parameter uncertainty of {SystemHandler.interbreed_param_uncertainty}.")
                    SystemHandler.do_interbreed = True
                    i = i + 2
                case float(6): #do_generation_step
                    print(f"Generation advance requested! Reinitializing socket handlers with child generation neural models.")
                    SystemHandler.apply_next_generation = True
            i+=1
        #print("SystemHandler was invoked.")
        return

class Brain(IOHandler):
    def __init__(self, socketID):
        super(Brain, self).__init__(socketID)
        if(not hasattr(self, "initmin")):
            self.initmin = -0.05
        if(not hasattr(self, "initmax")):
            self.initmax = 0.05
        self.model = self._setup_model()
        self.fitness = 0
        self.best = -1
    def _setup_model(self):
        return None
    def process_system_codes(self, codes):
        i = 1
        while i < len(codes):
            match codes[i]:
                case float(1): #Set fitness value
                    self.fitness = codes[i + 1]
                    i += 1
            i += 1
        self.nextOutput = array.array('d', [sys.float_info.max] * 128)
        #print(f"System codes processed at handler {self.socketID}.")
    def save_weights(self):
        print(f"Saving weights for Brain model with ID {self.socketID}...")
        self.model.save(f"./PyModelWeights/Simulation_{str(SystemHandler.simcode)}/brain_{self.socketID}", overwrite=True)
    def load_weights(self, simcode, socketID):
        print(f"Loading weights to Brain model with ID {socketID}...")
        try:
            self.model = tf.keras.models.load_model(f"./PyModelWeights/Simulation_{str(simcode)}/brain_{socketID}")
        except:
            try:
                self.model = tf.keras.models.load_model(f"./PyModelWeights/Simulation_{str(simcode)}.0/brain_{socketID}")
            except:
                print(f"Could not load brain {socketID}.")
    def handle_input(self, doubles):
        #print(f"Handling input for {self.socketID}.")
        if doubles[0] == sys.float_info.max:
            self.process_system_codes(doubles)
        else:
            if SystemHandler.runsim:
                infer = [1]
                infer.extend(np.array(self.model(np.array([doubles[0:world_dim]]))).tolist()[0])
                self.nextOutput = array.array('d', infer)
            else:
                self.nextOutput = array.array('d', [float(0)] * 128)
    def copy(self):
        copy_of_self = type(self)(self.socketID)
        copy_of_self.model = tf.keras.models.clone_model(self.model)
        copy_of_self.best = self.best
        return copy_of_self

class SimpleFeedForwardDense(Brain):
    def __init__(self, socketID):
        self.initmin = -1
        self.initmax = 1
        super(SimpleFeedForwardDense, self).__init__(socketID)

    def _setup_model(self):
        trueWorldInputs = tf.keras.Input(shape=(world_dim))

        worldInputs = tf.keras.layers.Dense(
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="worldInputs")(trueWorldInputs)


        x = tf.keras.layers.Dense(
            units = world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="x1"
        )(worldInputs)

        #x = tf.keras.layers.Dense(
        #    units=world_dim,
        #    activation='relu',
        #    kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    name="x2"
        #)(x)

        #x = tf.keras.layers.Dense(
        #    units=world_dim,
        #    activation='relu',
        #    kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
        #    name="x3"
        #)(x)

        forMovement = tf.keras.layers.Dense(
            units=movement_neurons,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="forMovement")(x)

        fullModel = tf.keras.Model(
            inputs=[worldInputs],
            outputs=[forMovement])
        #fullModel.summary()
        weights = fullModel.get_weights()

        #print(len(fullModel.get_weights()))
        #i = 0
        #for layerWeights in weights:
        #    i+=1
        #    print(f"Layer {i}: {len(layerWeights)}")

        return fullModel

class IterativeMemoryTransformer(Brain):
    def __init__(self, socketID):
        self.initmin = -1
        self.initmax = 1
        super(IterativeMemoryTransformer, self).__init__(socketID)
        self.memory_box = ["X"] * memorybox_sequence_length

    def handle_input(self, doubles):
        if doubles[0] == sys.float_info.max:
            self.process_system_codes(doubles)
        else:
            if SystemHandler.runsim:
                model_run = self.model([np.array([doubles[0:world_dim]]), np.array([''.join(self.memory_box)])])
                model_outs = []
                for tensor in model_run:
                    model_outs.append(np.array(tensor).tolist()[0])
                infer = [1]
                infer.extend(model_outs[0])
                self.nextOutput = array.array('d', infer)
                if(model_outs[3][0] > 0.5):
                    index = 0
                    largest = 0
                    write_index = -1
                    for prob in model_outs[2]:
                        if prob > largest:
                            largest = prob
                            write_index = index
                        index += 1
                    index = 0
                    largest = 0
                    val_index = -1
                    for prob in model_outs[1]:
                        if prob > largest:
                            largest = prob
                            val_index = index
                        index += 1
                    self.memory_box[write_index] = asciiChars[val_index]
                if(len(model_outs) > 4 and model_outs[6][0] > 0.5):
                    index = 0
                    largest = 0
                    write_index = -1
                    for prob in model_outs[5]:
                        if prob > largest:
                            largest = prob
                            write_index = index
                        index += 1
                    index = 0
                    largest = 0
                    val_index = -1
                    for prob in model_outs[4]:
                        if prob > largest:
                            largest = prob
                            val_index = index
                        index += 1
                    self.memory_box[write_index] = asciiChars[val_index]
                #if(self.socketID == 1):
                    #print(self.memory_box)
                    #print(self.nextOutput)
            else:
                self.nextOutput = array.array('d', [float(0)] * 128)

    def _setup_model(self):
        trueWorldInputs = tf.keras.Input(shape=(world_dim))

        worldInputs = tf.keras.layers.Dense(
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="worldInputs")(trueWorldInputs)

        boxInput = tf.keras.Input(
            shape=(1,),
            dtype=tf.string)

        #print(boxInput.shape)

        textVec = tf.keras.layers.TextVectorization(
            max_tokens=len(asciiR) + 2,
            output_mode='int',
            output_sequence_length=memorybox_sequence_length,
            standardize=None,
            split="character",
            vocabulary=asciiR,
            name="textVec")(boxInput)

        #print(textVec.shape)

        textEmb = tf.keras.layers.Embedding(
            input_dim=len(asciiR) + 3,
            output_dim=embed_dim,
            embeddings_initializer=tf.keras.initializers.random_normal,
            input_length=memorybox_sequence_length,
            name="textEmb")(textVec)

        expandForReshape = tf.keras.layers.Dense(
            units = world_dim * embed_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="expandForReshape")(worldInputs)

        embedFlat = tf.keras.layers.Reshape((memorybox_sequence_length * embed_dim,))(textEmb)

        confluence = tf.keras.layers.Dense(
            units=8 * embed_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="confluence")(tf.keras.layers.concatenate([expandForReshape, embedFlat], name="conflConcat"))

        reExpand = tf.keras.layers.Reshape((8, embed_dim),
                                           name="reExpand")(confluence)

        attend = tf.keras.layers.MultiHeadAttention(
            num_heads=attention_heads,
            key_dim=int(embed_dim / attention_heads),
            name="attend"
        )(reExpand, reExpand)

        add = tf.keras.layers.Add(name="add")([attend, reExpand])

        norm = tf.keras.layers.LayerNormalization(axis=[1, 2],
                                                  name="norm")(add)

        x = tf.keras.layers.Dense(
            units = world_dim * embed_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="x1"
        )(expandForReshape)
        x = tf.keras.layers.Dense(
            units=world_dim * embed_dim,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="x2"
        )(x)
        x = tf.keras.layers.Dense(
            units=world_dim * embed_dim,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="x3"
        )(x)

        flatAttn = tf.keras.layers.Flatten(name="flatAttn")(norm)

        carry = tf.keras.layers.Dense(
            units = 12,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="carry"
        )(tf.keras.layers.concatenate([x, flatAttn], name="carryConcat"))

        forMovement = tf.keras.layers.Dense(
            units=movement_neurons,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="forMovement")(carry)

        writeValueOne = tf.keras.layers.Dense(
            units=len(asciiChars),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeValueOne")(carry)

        writeValueOutOne = tf.keras.layers.Softmax(name="writeValueOutOne")(writeValueOne)

        writePosOne = tf.keras.layers.Dense(
            units=memorybox_sequence_length,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writePosOne")(carry)

        writePosOutOne = tf.keras.layers.Softmax(name="writePosOutOne")(writePosOne)

        writeBoolOne = tf.keras.layers.Dense(
            units=1,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeBoolOne")(carry)

        writeValueTwo = tf.keras.layers.Dense(
            units=len(asciiChars),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeValueTwo")(carry)

        writeValueOutTwo = tf.keras.layers.Softmax(name="writeValueOutTwo")(writeValueTwo)

        writePosTwo = tf.keras.layers.Dense(
            units=memorybox_sequence_length,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writePosTwo")(carry)

        writePosOutTwo = tf.keras.layers.Softmax(name="writePosOutTwo")(writePosTwo)

        writeBoolTwo = tf.keras.layers.Dense(
            units=1,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeBoolTwo")(carry)

        writeValueThree = tf.keras.layers.Dense(
            units=94,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writeValueThree")(carry)

        writeValueOutThree = tf.keras.layers.Softmax(name="writeValueOutThree")(writeValueThree)

        writePosThree = tf.keras.layers.Dense(
            units=memorybox_sequence_length,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writePosThree")(carry)

        writePosOutThree = tf.keras.layers.Softmax(name="writePosOutThree")(writePosThree)

        writeBoolThree = tf.keras.layers.Dense(
            units=1,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writeBoolThree")(carry)

        fullModel = tf.keras.Model(
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

class IMemBoT2(IterativeMemoryTransformer):
    def __init__(self, socketID):
        super(IMemBoT2, self).__init__(socketID)

    def _setup_model(self):
        trueWorldInputs = tf.keras.Input(shape=(world_dim))

        worldInputs = tf.keras.layers.Dense(
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

        boxInput = tf.keras.Input(
            shape=(1,),
            dtype=tf.string)

        #print(boxInput.shape)

        textVec = tf.keras.layers.TextVectorization(
            max_tokens=len(asciiR) + 2,
            output_mode='int',
            output_sequence_length=memorybox_sequence_length,
            standardize=None,
            split="character",
            vocabulary=asciiR,
            name="textVec")(boxInput)

        #print(textVec.shape)

        textEmb = tf.keras.layers.Embedding(
            input_dim=len(asciiR) + 3,
            output_dim=embed_dim,
            embeddings_initializer=tf.keras.initializers.random_normal,
            input_length=memorybox_sequence_length,
            name="textEmb")(textVec)

        attend = tf.keras.layers.MultiHeadAttention(
            num_heads=attention_heads,
            key_dim=int(embed_dim / attention_heads),
            name="attend"
        )(textEmb, textEmb)

        add = tf.keras.layers.Add(name="add")([attend, textEmb])

        norm = tf.keras.layers.LayerNormalization(axis=[1, 2],
                                                  name="norm")(add)

        flatAttn = tf.keras.layers.Flatten(name="flatAttn")(norm)

        carry = tf.keras.layers.Dense(
            units = 8,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="carry"
        )(tf.keras.layers.concatenate([worldInputs, flatAttn], name="carryConcat"))

        forMovement = tf.keras.layers.Dense(
            units=movement_neurons,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="forMovement")(carry)

        writeValueOne = tf.keras.layers.Dense(
            units=len(asciiChars),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeValueOne")(carry)

        writeValueOutOne = tf.keras.layers.Softmax(name="writeValueOutOne")(writeValueOne)

        writePosOne = tf.keras.layers.Dense(
            units=memorybox_sequence_length,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writePosOne")(carry)

        writePosOutOne = tf.keras.layers.Softmax(name="writePosOutOne")(writePosOne)

        writeBoolOne = tf.keras.layers.Dense(
            units=1,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeBoolOne")(carry)

        fullModel = tf.keras.Model(
            inputs=[worldInputs, boxInput],
            outputs=[forMovement,
                     writeValueOutOne, writePosOutOne, writeBoolOne])
        #fullModel.summary()
        #weights = fullModel.get_weights()

        #print(len(fullModel.get_weights()))
        #for layerWeights in weights:
        #    print(len(layerWeights))

        return fullModel

#base_model = tf.keras.models.load_model(f"./PyModelWeights/Simulation_1/brain_4")

class ChimericMemBoT(IterativeMemoryTransformer):
    def __init__(self, socketID):
        super(ChimericMemBoT, self).__init__(socketID)

    def _setup_model(self):

        #prime with pretrained

        #print(f"Loading weights to Brain model with ID {self.socketID}...")

        trueWorldInputs = tf.keras.Input(shape=(world_dim))

        worldInputs = tf.keras.layers.Dense(
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="worldInputs")(trueWorldInputs)

        x = tf.keras.layers.Dense(
            units = world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="x1"
        )(worldInputs)

        boxInput = tf.keras.Input(
            shape=(1,),
            dtype=tf.string)

        #print(boxInput.shape)

        textVec = tf.keras.layers.TextVectorization(
            max_tokens=len(asciiR) + 2,
            output_mode='int',
            output_sequence_length=memorybox_sequence_length,
            standardize=None,
            split="character",
            vocabulary=asciiR,
            name="textVec")(boxInput)

        #print(textVec.shape)

        textEmb = tf.keras.layers.Embedding(
            input_dim=len(asciiR) + 3,
            output_dim=embed_dim,
            embeddings_initializer=tf.keras.initializers.random_normal,
            input_length=memorybox_sequence_length,
            name="textEmb")(textVec)

        attend = tf.keras.layers.MultiHeadAttention(
            num_heads=attention_heads,
            key_dim=int(embed_dim / attention_heads),
            name="attend"
        )(textEmb, textEmb)

        add = tf.keras.layers.Add(name="add")([attend, textEmb])

        norm = tf.keras.layers.LayerNormalization(axis=[1, 2],
                                                  name="norm")(add)

        flatAttn = tf.keras.layers.Flatten(name="flatAttn")(norm)

        attention_extend = len(flatAttn.shape)

        prederive = tf.keras.layers.concatenate([x, flatAttn], name="prederive") 

        forMovement = tf.keras.layers.Dense(
            units=movement_neurons,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.zeros(),
            bias_initializer=tf.keras.initializers.zeros(),
            name="forMovement")(prederive)

        writeValueOne = tf.keras.layers.Dense(
            units=len(asciiChars),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeValueOne")(prederive)

        writeValueOutOne = tf.keras.layers.Softmax(name="writeValueOutOne")(writeValueOne)

        writePosOne = tf.keras.layers.Dense(
            units=memorybox_sequence_length,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writePosOne")(prederive)

        writePosOutOne = tf.keras.layers.Softmax(name="writePosOutOne")(writePosOne)

        writeBoolOne = tf.keras.layers.Dense(
            units=1,
            activation='sigmoid',
            kernel_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            bias_initializer=tf.keras.initializers.random_uniform(self.initmin, self.initmax),
            name="writeBoolOne")(prederive)

        fullModel = tf.keras.Model(
            inputs=[worldInputs, boxInput],
            outputs=[forMovement,
                     writeValueOutOne, writePosOutOne, writeBoolOne])

        base_weights = base_model.get_weights()
        layers = fullModel.layers
        for layer in layers:
            match layer.name:
                case "x1":
                    layer.set_weights([base_weights[0], base_weights[1]])
                case "forMovement":
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