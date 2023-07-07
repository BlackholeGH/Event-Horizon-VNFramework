import array
import random
import sys
import math
import os

import numpy as np
import keras
import tensorflow as tf

import logging
logging.getLogger('tensorflow').setLevel(logging.ERROR)

asciiChars = '!"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~'
asciiR = np.array(list(map(str, list(asciiChars))))
print(asciiR)
print(len(asciiR))

world_dim = 8
embed_dim = 4
memorybox_sequence_length = 16
attention_heads = 2
movement_neurons = 2

def do_one_crossbreed(brain_one, brain_two, proportion_one_to_two):
    if(not isinstance(brain_two, type(brain_one))):
        return None
    else:
        brain_child = type(brain_one)(-1)
        model_one_weights = brain_one.model.get_weights()
        model_two_weights = brain_two.model.get_weights()
        child_weights = brain_child.model.get_weights()
        layerindex = 0
        for layer in child_weights:
            for index, weight in np.ndenumerate(layer):
                if random.random() <= proportion_one_to_two:
                    layer[index] = model_one_weights[layerindex][index]
                else:
                    layer[index] = model_two_weights[layerindex][index]
            layerindex += 1
        brain_child.model.set_weights(child_weights)
        return brain_child

def interbreed_by_fitness(brains):
    canonical_brain_type = type(brains[0])
    active_socket_ids = []
    for brain in brains:
        active_socket_ids.append(brain.socketID)
        if not isinstance(brain, canonical_brain_type):
            return None
    sorted_brains = sorted(brains, key=lambda brain: brain.fitness, reverse=True)
    surviving_brains = sorted_brains[0:math.floor(len(sorted_brains) / 2)]
    for brain in sorted_brains:
        rank = sorted_brains.index(brain) + 1
        spouse_index = math.floor(random.triangular(0, len(sorted_brains), 0))
        spouse = sorted_brains[spouse_index]
        child = do_one_crossbreed(brain, spouse, 1 / (rank / (spouse_index + 1)))
        surviving_brains.append(child)
    while len(surviving_brains) < len(sorted_brains):
        surviving_brains.append(canonical_brain_type(-1))
    index = 0
    for new_generation_brain in surviving_brains:
        new_generation_brain.socketID = active_socket_ids[index]
        index += 1
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
                    print(f"Interbreed event requested! Generating next generation based on current fitness values.")
                    do_interbreed = True
                case float(6): #do_generation_step
                    print(f"Generation advance requested! Reinitializing socket handlers with child generation neural models.")
                    apply_next_generation = True
            i+=1

        self.nextOutput = array.array('d', [float(0)] * 128)
        #print("SystemHandler was invoked.")
        return

class Brain(IOHandler):
    def __init__(self, socketID):
        super(Brain, self).__init__(socketID)
        self.model = self._setup_model()
        self.fitness = 0
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
    def save_weights(self):
        print(f"Saving weights for Brain model with ID {self.socketID}...")
        self.model.save(f"./PyModelWeights/Simulation_{str(SystemHandler.simcode)}/brain_{self.socketID}", overwrite=True)
    def load_weights(self, simcode, socketID):
        print(f"Loading weights to Brain model with ID {self.socketID}...")
        self.model = tf.keras.models.load_model(f"./PyModelWeights/Simulation_{str(simcode)}/brain_{self.socketID}")
    def handle_input(self, doubles):
        if doubles[0] == sys.float_info.max:
            self.process_system_codes(doubles)
        else:
            if SystemHandler.runsim:
                #print(f"Handling input starting with {str(doubles[0])}")
                infer = np.array(self.model(np.array([doubles[0:8]]))).tolist()[0]
                self.nextOutput = array.array('d', infer)
            else:
                self.nextOutput = array.array('d', [float(0)] * 128)

class SimpleFeedForwardDense(Brain):
    def __init__(self, socketID):
        super(SimpleFeedForwardDense, self).__init__(socketID)

    def _setup_model(self):
        """
        Let's think about this.
        Three read heads from the memory segment?
        Inputs:

        ForwardAlignedVelocity
        StrafeVelocity
        FrontDistance
        FrontLeftDistance
        FrontRightDistance
        FrontType
        FrontLeftType
        FrontRightType

        Outputs:

        Write1 Index
        Write1 Data
        Write2 Index
        Write2 Data
        Write3 Index
        Write3 Data

        Forward
        Turn
        :return: 
        """
        trueWorldInputs = tf.keras.Input(shape=(world_dim))

        worldInputs = tf.keras.layers.Dense(
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="worldInputs")(trueWorldInputs)


        x = tf.keras.layers.Dense(
            units = world_dim,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="x1"
        )(worldInputs)

        x = tf.keras.layers.Dense(
            units=world_dim,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="x2"
        )(x)

        x = tf.keras.layers.Dense(
            units=world_dim,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="x3"
        )(x)

        forMovement = tf.keras.layers.Dense(
            units=movement_neurons,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="forMovement")(x)

        fullModel = tf.keras.Model(
            inputs=[worldInputs],
            outputs=[forMovement])
        #fullModel.summary()
        #weights = fullModel.get_weights()

        #print(len(fullModel.get_weights()))
        #for layerWeights in weights:
        #    print(len(layerWeights))

        return fullModel

class IterativeMemoryTransformer(Brain):
    def __init__(self, socketID):
        super(IterativeMemoryTransformer, self).__init__(socketID)

    def _setup_model(self):
        """
        Let's think about this.
        Three read heads from the memory segment?
        Inputs:

        ForwardAlignedVelocity
        StrafeVelocity
        FrontDistance
        FrontLeftDistance
        FrontRightDistance
        FrontType
        FrontLeftType
        FrontRightType

        Outputs:

        Write1 Index
        Write1 Data
        Write2 Index
        Write2 Data
        Write3 Index
        Write3 Data

        Forward
        Turn
        :return: 
        """
        trueWorldInputs = tf.keras.Input(shape=(world_dim,))

        worldInputs = tf.keras.layers.Dense(
            units=world_dim,
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="worldInputs")(trueWorldInputs)

        boxInput = tf.keras.Input(
            shape=(memorybox_sequence_length),
            dtype=tf.string)

        print(boxInput.shape)

        textVec = tf.keras.layers.TextVectorization(
            max_tokens=len(asciiR) + 2,
            output_mode='int',
            standardize=None,
            split=None,
            vocabulary=asciiR,
            name="textVec")(boxInput)

        print(textVec.shape)

        textEmb = tf.keras.layers.Embedding(
            input_dim=len(asciiR) + 3,
            output_dim=embed_dim,
            embeddings_initializer=tf.keras.initializers.random_normal,
            input_length=memorybox_sequence_length,
            name="textEmb")(textVec)

        expandForReshape = tf.keras.layers.Dense(
            units = world_dim * embed_dim,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="expandForReshape")(worldInputs)

        confluence = tf.keras.layers.Dense(
            units=(embed_dim * memorybox_sequence_length) + (embed_dim * world_dim),
            activation='relu',
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="confluence")(tf.keras.layers.concatenate([expandForReshape, tf.keras.layers.Flatten()(textEmb)]))

        reExpand = tf.keras.layers.Reshape((memorybox_sequence_length + world_dim, embed_dim),
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
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
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
            units = (world_dim * embed_dim) + (embed_dim * (memorybox_sequence_length + world_dim)),
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="carry"
        )(tf.keras.layers.concatenate([x, flatAttn]))

        forMovement = tf.keras.layers.Dense(
            units=movement_neurons,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="forMovement")(carry)

        writeValueOne = tf.keras.layers.Dense(
            units=94,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writeValueOne")(carry)

        writeValueOutOne = tf.keras.layers.Softmax(name="writeValueOutOne")(writeValueOne)

        writePosOne = tf.keras.layers.Dense(
            units=memorybox_sequence_length,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writePosOne")(carry)

        writePosOutOne = tf.keras.layers.Softmax(name="writePosOutOne")(writePosOne)

        writeBoolOne = tf.keras.layers.Dense(
            units=1,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writeBoolOne")(carry)

        writeValueTwo = tf.keras.layers.Dense(
            units=94,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writeValueTwo")(carry)

        writeValueOutTwo = tf.keras.layers.Softmax(name="writeValueOutTwo")(writeValueTwo)

        writePosTwo = tf.keras.layers.Dense(
            units=memorybox_sequence_length,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
            name="writePosTwo")(carry)

        writePosOutTwo = tf.keras.layers.Softmax(name="writePosOutTwo")(writePosTwo)

        writeBoolTwo = tf.keras.layers.Dense(
            units=1,
            kernel_initializer=tf.keras.initializers.random_normal,
            bias_initializer=tf.keras.initializers.zeros,
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
                     writeValueOutTwo, writePosOutTwo, writeBoolTwo,
                     writeValueOutThree, writePosOutThree, writeBoolThree])
        fullModel.summary()
        weights = fullModel.get_weights()

        print(len(fullModel.get_weights()))
        for layerWeights in weights:
            print(len(layerWeights))

        return fullModel

#imembot = IterativeMemoryTransformer(0)
#sffd = SimpleFeedForwardDense(0)