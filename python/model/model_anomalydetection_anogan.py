import tensorflow as tf

from util.helper import residual_block
from util.helper import transition_down_expandChannelDouble
from util.helper import transition_up


class model_anomalydetection_anogan:

    def __init__(self, sess, name):
        self.sess = sess
        self.name = name
        self._build_net()

    def generator(self, input):
        with tf.variable_scope('generator'):

            # 256x256x8
            encode1 = tf.layers.separable_conv2d(input,
                                                 filters=8,
                                                 kernel_size=[3, 3],
                                                 strides=[1, 1],
                                                 use_bias=False, padding='SAME', activation=None,
                                                 pointwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                 depthwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                 name='encode1_1')

            # 256x256x8
            encode1 = residual_block('encode1_2', encode1, 8, self.keep_layer)
            # 128x128x32
            transition_down1 = transition_down_expandChannelDouble('transition_down1', encode1, 32, self.keep_layer)
            print(transition_down1)

            # 128x128x32
            encode2 = residual_block('encode2', transition_down1, 32, self.keep_layer)
            # 64x64x64
            transition_down2 = transition_down_expandChannelDouble('transition_down2', encode2, 64, self.keep_layer)
            print(transition_down2)

            # 64x64x64
            encode3 = residual_block('encode3', transition_down2, 64, self.keep_layer)
            # 32x32x128
            transition_down3 = transition_down_expandChannelDouble('transition_down3', encode3, 128, self.keep_layer)
            print(transition_down3)

            # 32x32x128
            encode4 = residual_block('encode4_1', transition_down3, 128, self.keep_layer)
            # 16x16x128
            transition_down4 = transition_down_expandChannelDouble('transition_down4', encode4, 128, self.keep_layer)
            print(transition_down4)

            # 16x16x128 (x3)
            encode5 = residual_block('encode5_1', transition_down4, 128, self.keep_layer)
            encode5 = residual_block('encode5_2', encode5, 128, self.keep_layer)
            #encode5 = residual_block('encode5_3', encode5, 128, self.keep_layer)
            # 32x32x128
            transition_up1 = transition_up('transition_up1', encode5, 128)
            print(transition_up1)

            # 32x32x128 (x3)
            decode1 = transition_up1 #+ encode4
            decode1 = residual_block('decode_1_1', decode1, 128, self.keep_layer)
            decode1 = residual_block('decode_1_2', decode1, 128, self.keep_layer)
            #decode1 = residual_block('decode_1_3', decode1, 128, self.keep_layer)
            # 64x64x64
            transition_up2 = transition_up('transition_up2', decode1, 64)
            print(transition_up2)

            # 64x64x64 (x3)
            decode2 = transition_up2 #+ encode3
            decode2 = residual_block('decode_2_1', decode2, 64, self.keep_layer)
            decode2 = residual_block('decode_2_2', decode2, 64, self.keep_layer)
            #decode2 = residual_block('decode_2_3', decode2, 64, self.keep_layer)
            # 128x128x32
            transition_up3 = transition_up('transition_up3', decode2, 32)
            print(transition_up3)

            # 128x128x32 (x3)
            decode3 = transition_up3 #+ encode2
            decode3 = residual_block('decode_3_1', decode3, 32, self.keep_layer)
            decode3 = residual_block('decode_3_2', decode3, 32, self.keep_layer)
            #decode3 = residual_block('decode_3_3', decode3, 32, self.keep_layer)
            # 256x256x8
            transition_up4 = transition_up('transition_up4', decode3, 8)
            print(transition_up4)

            # 256x256x8 (x3)
            decode4 = transition_up4 #+ encode1
            decode4 = residual_block('decode_4_1', decode4, 8, self.keep_layer)
            decode4 = residual_block('decode_4_2', decode4, 8, self.keep_layer)
            #decode4 = residual_block('decode_4_3', decode4, 8, self.keep_layer)

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
            decode5 = tf.nn.sigmoid(decode5)
            return decode5

    def discriminator(self, input, reuse=None):
        with tf.variable_scope("discriminator") as scope:

            if reuse:
                scope.reuse_variables()

            # 256x256x8
            encode1 = residual_block('encode1_2', input, 8, self.keep_layer)
            # 128x128x32
            transition_down1 = transition_down_expandChannelDouble('transition_down1', encode1, 32, self.keep_layer)
            print(transition_down1)

            # 128x128x32
            encode2 = residual_block('encode2', transition_down1, 32, self.keep_layer)
            # 64x64x64
            transition_down2 = transition_down_expandChannelDouble('transition_down2', encode2, 64, self.keep_layer)
            print(transition_down2)

            # 64x64x64
            encode3 = residual_block('encode3', transition_down2, 64, self.keep_layer)
            # 32x32x128
            transition_down3 = transition_down_expandChannelDouble('transition_down3', encode3, 128, self.keep_layer)
            print(transition_down3)

            # 32x32x128
            encode4 = residual_block('encode4_1', transition_down3, 128, self.keep_layer)
            # 16x16x128
            transition_down4 = transition_down_expandChannelDouble('transition_down4', encode4, 128, self.keep_layer)
            print(transition_down4)

            # 16x16x128 (x3)
            encode5 = residual_block('encode5_1', transition_down4, 128, self.keep_layer)
            encode5 = residual_block('encode5_2', encode5, 1, self.keep_layer)
            net = tf.compat.v1.nn.avg_pool2d(value=encode5, ksize=[1, 16, 16, 1], strides=[1, 1, 1, 1], padding='VALID')
            net = tf.compat.v1.layers.flatten(net)
            return net


    def _build_net(self):
        #with tf.variable_scope(self.name):
        print('=== network structure ===')

        self.X = tf.placeholder(tf.float32, [None, 256, 256, 1], name='RealTarget')
        self.Z = tf.placeholder(tf.float32, [None, 256, 256, 1], name='RealInput')
        self.learning_rate_tensor = tf.compat.v1.placeholder(tf.float32, shape=[], name='learning_rate')
        self.keep_layer = tf.placeholder(tf.bool, name='phase')

        self.G = self.generator(self.Z)
        self.D_real = self.discriminator(self.X)
        self.D_gene = self.discriminator(self.G, True)
        self.output = self.G

        self.loss_D_real = tf.reduce_mean(tf.nn.sigmoid_cross_entropy_with_logits(logits=self.D_real, labels=tf.ones_like(self.D_real)))
        self.loss_D_gene = tf.reduce_mean(tf.nn.sigmoid_cross_entropy_with_logits(logits=self.D_gene, labels=tf.zeros_like(self.D_gene)))

        self.loss_D = self.loss_D_real + self.loss_D_gene
        self.loss_G = tf.reduce_mean(tf.nn.sigmoid_cross_entropy_with_logits(logits=self.D_gene, labels=tf.ones_like(self.D_gene)))

        vars_D = tf.get_collection(tf.GraphKeys.TRAINABLE_VARIABLES, scope='discriminator')
        vars_G = tf.get_collection(tf.GraphKeys.TRAINABLE_VARIABLES, scope='generator')
        print('=== network structure ===')

        update_ops = tf.get_collection(tf.GraphKeys.UPDATE_OPS)
        with tf.control_dependencies(update_ops):
            self.train_D = tf.train.AdamOptimizer().minimize(self.loss_D, var_list=vars_D)
            self.train_G = tf.train.AdamOptimizer().minimize(self.loss_G, var_list=vars_G)



        pre = tf.cast(self.output > 0.5, dtype=tf.float32)
        truth = tf.cast(self.Z > 0.5, dtype=tf.float32)
        inse = tf.reduce_sum(tf.multiply(pre, truth), axis=(1, 2, 3))  # AND
        union = tf.reduce_sum(tf.cast(tf.add(pre, truth) >= 1, dtype=tf.float32), axis=(1, 2, 3))  # OR
        batch_iou = (inse + 1e-5) / (union + 1e-5)
        self.accuracy = tf.reduce_mean(batch_iou, name='iou_coe1')


        print("==============Node Name List==============")
        print("learning  rate tensor : ", self.learning_rate_tensor)
        print("Input Node Name : ", self.X)
        print("Phase Node Name", self.keep_layer)
        print("Accuracy Node Name : ", self.accuracy)
        print("Output Node Name : ", self.output)
        print("==============Node Name List==============")


    def reconstruct(self, x_test, keep_prop=False):
        return self.sess.run(self.output, feed_dict={self.Z: x_test, self.keep_layer: keep_prop})

    def train_discriminator(self, x_data, keep_prop=True, learning_rate=0.003):
        return self.sess.run([self.train_D, self.loss_D], feed_dict={self.X: x_data, self.Z: x_data, self.keep_layer: keep_prop, self.learning_rate_tensor:learning_rate})

    def train_generator(self, x_data, keep_prop=True, learning_rate=0.003):
        return self.sess.run([self.train_G, self.loss_G], feed_dict={self.X: x_data, self.Z: x_data, self.keep_layer: keep_prop, self.learning_rate_tensor:learning_rate})

    def get_accuracy(self, x_test, keep_prop=False):
        return self.sess.run(self.accuracy, feed_dict={self.Z: x_test, self.keep_layer: keep_prop})




