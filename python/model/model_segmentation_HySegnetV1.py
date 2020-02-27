import tensorflow as tf

from util.helper import residual_block
from util.helper import transition_down_expandChannelDouble
from util.helper import transition_up


class model_segmentation_HySegnetV1:

    def __init__(self, sess, name):
        self.sess = sess
        self.name = name
        self._build_net()

    def _build_net(self):
        with tf.variable_scope(self.name):
            # placeholder 100x100 = 10000
            self.X = tf.placeholder(tf.float32, [None, 256, 256, 3], name='input')
            self.Y = tf.placeholder(tf.float32, [None, 256, 256, 1], name='output')
            self.learning_rate_tensor = tf.compat.v1.placeholder(tf.float32, shape=[], name='learning_rate')
            self.keep_layer = tf.placeholder(tf.bool, name='phase')

            print('=== network structure ===')
            encode1 = tf.layers.separable_conv2d(self.X,
                                                 filters=8,
                                                 kernel_size=[3, 3],
                                                 strides=[1, 1],
                                                 use_bias=False, padding='SAME', activation=None,
                                                 pointwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                 depthwise_initializer=tf.compat.v1.variance_scaling_initializer(), name='encode1_1')

            encode1 = residual_block('encode1_2', encode1, 8, self.keep_layer)
            transition_down1 = transition_down_expandChannelDouble('transition_down1', encode1, 32, self.keep_layer)
            print(transition_down1)

            encode2 = residual_block('encode2', transition_down1, 32, self.keep_layer)
            transition_down2 = transition_down_expandChannelDouble('transition_down2', encode2, 64, self.keep_layer)
            print(transition_down2)

            encode3 = residual_block('encode3', transition_down2, 64, self.keep_layer)
            transition_down3 = transition_down_expandChannelDouble('transition_down3', encode3, 128, self.keep_layer)
            print(transition_down3)

            encode4 = residual_block('encode4_1', transition_down3, 128, self.keep_layer)
            encode4 = residual_block('encode4_2', encode4, 128, self.keep_layer)
            transition_down4 = transition_down_expandChannelDouble('transition_down4', encode4, 128, self.keep_layer)
            print(transition_down4)

            encode5 = residual_block('encode5_1', transition_down4, 128, self.keep_layer)
            encode5 = residual_block('encode5_2', encode5, 128, self.keep_layer)
            encode5 = residual_block('encode5_3', encode5, 128, self.keep_layer)
            encode5 = residual_block('encode5_4', encode5, 128, self.keep_layer)
            encode5 = residual_block('encode5_5', encode5, 128, self.keep_layer)
            encode5 = residual_block('encode5_6', encode5, 128, self.keep_layer)
            transition_up1 = transition_up('transition_up1', encode5, 128)
            print(transition_up1)

            decode1 = transition_up1 + encode4
            decode1 = residual_block('decode_1_1', decode1, 128, self.keep_layer)
            decode1 = residual_block('decode_1_2', decode1, 128, self.keep_layer)
            decode1 = residual_block('decode_1_3', decode1, 128, self.keep_layer)
            transition_up2 = transition_up('transition_up2', decode1, 64)
            print(transition_up2)

            decode2 = transition_up2 + encode3
            decode2 = residual_block('decode_2_1', decode2, 64, self.keep_layer)
            decode2 = residual_block('decode_2_2', decode2, 64, self.keep_layer)
            decode2 = residual_block('decode_2_3', decode2, 64, self.keep_layer)
            transition_up3 = transition_up('transition_up3', decode2, 32)
            print(transition_up3)

            decode3 = transition_up3 + encode2
            decode3 = residual_block('decode_3_1', decode3, 32, self.keep_layer)
            decode3 = residual_block('decode_3_2', decode3, 32, self.keep_layer)
            decode3 = residual_block('decode_3_3', decode3, 32, self.keep_layer)
            transition_up4 = transition_up('transition_up4', decode3, 8)
            print(transition_up4)

            decode4 = transition_up4 + encode1
            decode4 = residual_block('decode_4_1', decode4, 8, self.keep_layer)
            decode4 = residual_block('decode_4_2', decode4, 8, self.keep_layer)
            decode4 = residual_block('decode_4_3', decode4, 8, self.keep_layer)

            decode5 = tf.compat.v1.layers.separable_conv2d(decode4,
                                                           filters=1,
                                                           kernel_size=[3, 3],
                                                           strides=[1, 1],
                                                           use_bias=False,
                                                           padding='SAME',
                                                           activation=None,
                                                           pointwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                           depthwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                           name='decode5')
            self.output = tf.nn.sigmoid(decode5)
            print('=== network structure ===')

            pre = tf.cast(self.output > 0.5, dtype=tf.float32)
            truth = tf.cast(self.Y > 0.5, dtype=tf.float32)
            inse = tf.reduce_sum(tf.multiply(pre, truth), axis=(1, 2, 3))  # AND
            union = tf.reduce_sum(tf.cast(tf.add(pre, truth) >= 1, dtype=tf.float32), axis=(1, 2, 3))  # OR
            batch_iou = (inse + 1e-5) / (union + 1e-5)
            self.accuracy = tf.reduce_mean(batch_iou, name='iou_coe1')


            self.cost = tf.reduce_mean(tf.nn.sigmoid_cross_entropy_with_logits(labels=self.Y, logits=decode5))
            #self.cost = tf.reduce_mean(tf.reduce_mean(tf.squared_difference(self.output, self.Y_input), 1), name='mse')
            #self.cost = tf.sqrt(tf.reduce_mean(tf.pow(self.Y_input - self.output, 2)))
            update_ops = tf.compat.v1.get_collection(tf.GraphKeys.UPDATE_OPS)
            with tf.compat.v1.control_dependencies(update_ops):
                self.optimizer = tf.compat.v1.train.AdamOptimizer(learning_rate=self.learning_rate_tensor).minimize(self.cost)

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


    def reconstruct(self, x_test, keep_prop=False):
        return self.sess.run(self.output, feed_dict={self.X: x_test, self.keep_layer: keep_prop, self.keep_layer:keep_prop})

    def train(self, x_data, y_data, keep_prop=True, learning_rate=0.003):
        return self.sess.run([self.cost, self.optimizer],
                             feed_dict={self.X: x_data, self.Y: y_data, self.keep_layer: keep_prop, self.keep_layer: keep_prop, self.learning_rate_tensor:learning_rate})

    def get_accuracy(self, x_test, y_test, keep_prop=False):
        return self.sess.run(self.accuracy, feed_dict={self.X: x_test, self.Y: y_test, self.keep_layer: keep_prop, self.keep_layer:keep_prop})
