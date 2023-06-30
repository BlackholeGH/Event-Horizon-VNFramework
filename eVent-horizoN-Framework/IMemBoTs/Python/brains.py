import array
import random

import numpy as np
import keras
import tensorflow as tf

asciiChars = '!"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~'
asciiR = np.array(list(map(str, list(asciiChars))))
print(asciiR)
print(len(asciiR))

world_dim = 16
embed_dim = 4
memorybox_sequence_length = 16
attention_heads = 2
movement_neurons = 16

class IOHandler:
    def __init__(self, socketID):
        self.socketID = socketID
        self.nextOutput = None
    def handleInput(self, doubles):
        self.nextOutput = array.array('d', [3])
        #print("IOHandler was invoked.")
        return
    def getOutput(self):
        return self.nextOutput

class SystemHandler(IOHandler):
    def __init__(self, socketID):
        super(SystemHandler, self).__init__(socketID)
    def handleInput(self, doubles):
        self.nextOutput = doubles
        #print("SystemHandler was invoked.")
        return

class Brain(IOHandler):
    def __init__(self, socketID):
        super(Brain, self).__init__(socketID)
        self.model = self._setup_model()
    def _setup_model(self):
        return None

class IterativeMemoryTransformer(Brain):
    def __init__(self, socketID):
        super(IterativeMemoryTransformer, self).__init__(socketID)

    def _setup_model(self):
        """
        Let's think about this.
        Three read heads from the memory segment?
        Inputs:

        Mem1
        Mem2
        Mem3
        Rotation
        FrontDistance
        LeftDistance
        RightDistance
        BehindDistance
        FrontLeftDistance
        FrontRightDistance
        FrontType
        LeftType
        RightType
        BehindType
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

        return print("Test")

#imembot = IterativeMemoryTransformer(0)