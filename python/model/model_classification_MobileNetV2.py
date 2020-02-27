import tensorflow as tf

from util.helper import inverted_bottle_neck


# 0 cat , 1 dog,

class model_classification_MobileNetV2:

    def __init__(self, sess, class_count):
        self.sess = sess
        self.class_count = class_count

        with tf.variable_scope('MobileNetV2'):
            self._build_net()

    def _build_net(self):
        self.learning_rate_tensor = tf.compat.v1.placeholder(tf.float32, shape=[], name='learning_rate')
        print(self.learning_rate_tensor)

        self.X = tf.compat.v1.placeholder(tf.float32, [None, 224 * 224 * 3], name='X')
        print(self.X)

        self.keep_layer = tf.compat.v1.placeholder(tf.bool, name='phase')
        print(self.keep_layer)

        self.Y = tf.compat.v1.placeholder(tf.float32, [None, self.class_count], 'Y')
        print(self.Y)

        X_input = tf.compat.v1.reshape(self.X, [-1, 224, 224, 3])

        print("=== network structure ===")

        with tf.variable_scope('conv1'):
            net = tf.compat.v1.layers.conv2d(X_input, filters=32, kernel_size=3, strides=2, use_bias=False,
                                             kernel_initializer=tf.initializers.glorot_normal(), padding='same')
            net = tf.compat.v1.layers.batch_normalization(net, momentum=0.9, training=self.keep_layer)
            print("conv1=", net)

        net = inverted_bottle_neck(net, 1, 16, True, self.keep_layer, name="bottle_net1")
        net = inverted_bottle_neck(net, 6, 24, False, self.keep_layer, name="bottle_net2")
        net = inverted_bottle_neck(net, 6, 32, False, self.keep_layer, name="bottle_net3")
        net = inverted_bottle_neck(net, 6, 64, False, self.keep_layer, name="bottle_net4")
        net = inverted_bottle_neck(net, 6, 96, True, self.keep_layer, name="bottle_net5")
        net = inverted_bottle_neck(net, 6, 160, False, self.keep_layer, name="bottle_net6")
        net = inverted_bottle_neck(net, 6, 320, True, self.keep_layer, name="bottle_net7")

        # net = inverted_bottle_neck(net, 6, 96, True, self.keep_layer, name="bottle_net5")
        # net = inverted_bottle_neck(net, 6, 160, False, self.keep_layer, name="bottle_net6")
        # net = inverted_bottle_neck(net, 6, 320, True, self.keep_layer, name="bottle_net7")

        with tf.variable_scope('conv2'):
            net = tf.compat.v1.layers.conv2d(net, filters=self.class_count, kernel_size=3, strides=1, use_bias=False,
                                             kernel_initializer=tf.initializers.glorot_normal(), padding='same')
            net = tf.compat.v1.layers.batch_normalization(net, momentum=0.9, training=self.keep_layer)
            print("conv2=", net)

        net = tf.compat.v1.nn.avg_pool2d(value=net, ksize=[1, 7, 7, 1], strides=[1, 1, 1, 1], padding='VALID')
        net = tf.compat.v1.layers.flatten(net)
        print('avg pool = ', net)

        print("=== network structure ===")

        self.output = tf.nn.softmax(net, -1, name='output');
        correct_prediction = tf.equal(tf.argmax(self.output, 1), tf.argmax(self.Y, 1))

        self.accuracy = tf.compat.v1.reduce_mean(tf.cast(correct_prediction, tf.float32), name='accuracy')
        self.cost = tf.compat.v1.reduce_mean(
            tf.compat.v1.nn.softmax_cross_entropy_with_logits_v2(logits=net, labels=self.Y), name='cost')

        # define cost/loss & optimizer
        update_ops = tf.compat.v1.get_collection(tf.compat.v1.GraphKeys.UPDATE_OPS)
        with tf.compat.v1.control_dependencies(update_ops):
            self.optimizer = tf.compat.v1.train.AdamOptimizer(learning_rate=self.learning_rate_tensor).minimize(
                self.cost, name='AdamMinimize')

        print("==============Node Name List==============")
        print("learning  rate tensor : ", self.learning_rate_tensor)
        print("Input Node Name : ", self.X)
        print("Output 4 Train Node Name : ", self.Y)
        print("Phase Node Name", self.keep_layer)
        print("Accuracy Node Name : ", self.accuracy)
        print("Output Node Name : ", self.output)
        print("Cost Function Node Name : ", self.cost)
        print("Run this operation for a train step            :", self.optimizer.name)
        print("==============Node Name List==============")

        # Global Average Pooling
        # self.average_polling = tf.reduce_mean(hidden_layer19, axis=(1, 2), name="globalAveragePool")
        # print(self.average_polling)

    def predict(self, x_test, keep_prop=False):
        return self.sess.run(self.output, feed_dict={self.X: x_test, self.keep_layer: keep_prop})

    def get_cost_accuracy(self, x_test, y_test, keep_prop=False):
        # print(self.sess.run(self.output, feed_dict={self.X: x_test, self.keep_layer: keep_prop}))
        return self.sess.run([self.accuracy, self.cost],
                             feed_dict={self.X: x_test, self.Y: y_test, self.keep_layer: keep_prop})

    def train(self, x_data, y_data, keep_prop=True, learn_rate=0.003):
        return self.sess.run(self.optimizer, feed_dict={self.X: x_data, self.Y: y_data, self.keep_layer: keep_prop,
                                                        self.learning_rate_tensor: learn_rate})