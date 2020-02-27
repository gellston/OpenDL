import tensorflow as tf

from util.helper import focal_loss
from util.helper import conv_elu_bn
from util.helper import deconv_elu_bn
from util.helper import residual_block_elu
from util.helper import conv_elu
from util.helper import conv
from util.helper import reg_l1_loss
from util.helper import conv_bn
from util.helper import deconv
from util.helper import max_pool2d
from util.helper import upsample_layer
from util.helper import hourglass_module

# 0 cat , 1 dog,

class model_objectdetection_atrous_centernet_v2:

    def __init__(self, sess, class_count):
        self.sess = sess
        self.class_count = class_count
        self.up_sample_rate = 1
        self.feature_channels = 96
        self.hourglass_channel = 32

        with tf.variable_scope('CenterNet'):
            self._build_net()

    def _build_net(self):
        self.learning_rate_tensor = tf.compat.v1.placeholder(tf.float32, shape=[], name='learning_rate')
        print(self.learning_rate_tensor)

        self.X = tf.compat.v1.placeholder(tf.float32, [None, 512, 512, 3], name='X')
        print(self.X)

        self.keep_layer = tf.compat.v1.placeholder(tf.bool, name='phase')
        print(self.keep_layer)

        self.Y = tf.compat.v1.placeholder(tf.float32, [None, 128, 128, self.class_count], 'Y')
        self.SIZE = tf.compat.v1.placeholder(tf.float32, [None, 128, 128, 2], 'Y')
        print(self.Y)

        ## Batch , Height , Width, Class
        #X_input = tf.compat.v1.reshape(self.X, [-1, 512, 512, 3])
        #Y_input = tf.compat.v1.reshape(self.Y, [-1, 128, 128, self.class_count])


        # 512 512 -> 256x 256
        with tf.variable_scope('stage_2'):
            stage_2_1 = conv_bn(self.X,  filters=16, kernel_size=7, strides=2, training=self.keep_layer, name='stage_2_1')

        ## 256 256 -> 128 128
        with tf.variable_scope('stage2'):
            stage4_1 = conv_bn(stage_2_1,  filters=32, kernel_size=3, strides=2, training=self.keep_layer, name='stage_2_1')
            #maxpool1 = max_pool2d(stage4_1, pool_size=2, strides=2, name='maxpool1')

        # 128 x 128 -> 64x64  ///// Desired output heatmap size start point
        with tf.variable_scope('stage4'):
            stage8_1 = hourglass_module(stage4_1, hourglass_filter=self.hourglass_channel, out_filter=self.hourglass_channel, training=self.keep_layer, name='stage8_')
            maxpool2 = max_pool2d(stage8_1, pool_size=2, strides=2, name='maxpool2')


        #64x64-> 32x32 # residualnet Start point
        with tf.variable_scope('stage8'):
            stage16_1 = hourglass_module(maxpool2, hourglass_filter=self.hourglass_channel, out_filter=self.hourglass_channel, training=self.keep_layer, name='stage16_')
            maxpool3 = max_pool2d(stage16_1, pool_size=2, strides=2, name='maxpool3')


        #32x32-> 16x16 # final map
        with tf.variable_scope('stage16'):
            stage32_1 = hourglass_module(maxpool3, hourglass_filter=self.hourglass_channel, out_filter=self.hourglass_channel, training=self.keep_layer, name='stage32_')
            maxpool4 = max_pool2d(stage32_1, pool_size=2, strides=2, name='maxpool4')

        #16x16 input
        with tf.variable_scope('feature1'):
            feature1_1 = hourglass_module(maxpool4, hourglass_filter=self.hourglass_channel, out_filter=self.hourglass_channel, training=self.keep_layer, name='feature1_1_')
            feature1_2 = residual_block_elu(feature1_1, self.hourglass_channel, training=self.keep_layer, name='feature1_2')
            feature1_3 = residual_block_elu(feature1_2, self.hourglass_channel, training=self.keep_layer, name='feature1_3')
            up1 = upsample_layer(feature1_3, [32, 32])

            shortcut1 = hourglass_module(stage32_1, hourglass_filter=self.hourglass_channel, out_filter=self.hourglass_channel, training=self.keep_layer, name='short1')
            summation1 = up1 + shortcut1
            #----->32x32

        with tf.variable_scope('feature2'):
            feature2_1 = residual_block_elu(summation1, self.hourglass_channel, training=self.keep_layer, name='feature2_1')
            up2 = upsample_layer(feature2_1, [64, 64])

            shortcut2 = hourglass_module(stage16_1, hourglass_filter=self.hourglass_channel, out_filter=self.hourglass_channel, training=self.keep_layer, name='short2')
            summation2 = up2 + shortcut2
            # ----->64x64


        with tf.variable_scope('feature3'):
            feature3_1 = residual_block_elu(summation2, 32, training=self.keep_layer, name='feature3_1')
            up3 = upsample_layer(feature3_1, [128, 128])

            shortcut3 = hourglass_module(stage8_1, hourglass_filter=32, out_filter=32, training=self.keep_layer, name='short2')
            summation3 = up3 + shortcut3
            # ----->128x128

        linear_feature1 = conv_elu_bn(summation3, filters=self.hourglass_channel, training=self.keep_layer, kernel_size=3, strides=1, name='linear_feature1')
        linear_feature2 = conv_elu_bn(linear_feature1, filters=self.hourglass_channel, training=self.keep_layer, kernel_size=3, strides=1, name='linear_feature2')

        #with tf.variable_scope('feature_anaysis')



        print("=== network structure ===")

        with tf.variable_scope("detector"):
            self.cls = conv_elu_bn(linear_feature2, filters=self.feature_channels, training=self.keep_layer, kernel_size=3, strides=1, name='detector_convelu1')
            self.cls = conv(self.cls, filters=self.class_count, kernel_size=1, strides=1, name='detector_conv1')
            self.cls = tf.compat.v1.nn.sigmoid(self.cls, name="heatmap")

            #self.L2 = tf.reduce_mean((self.cls - self.Y ) * (self.cls - self.Y ))


            self.size = conv_elu_bn(linear_feature2, filters=self.feature_channels, training=self.keep_layer, kernel_size=3, strides=1, name='detector_convelu2')
            self.size = conv(self.size, filters=2, kernel_size=1, strides=1, name='detector_conv2')
            self.size = tf.compat.v1.nn.relu(self.size, name='sizemap')


            print("heatmap sigmoid=", self.cls)

        self.output = self.cls;
        print("=== network structure ===")


        self.heatmap_loss = focal_loss(self.output, self.Y)
        self.size_loss = reg_l1_loss(self.size, self.SIZE)
        self.cost = self.heatmap_loss + 0.1 * self.size_loss
        # define cost/loss & optimizer
        update_ops = tf.compat.v1.get_collection(tf.compat.v1.GraphKeys.UPDATE_OPS)
        with tf.compat.v1.control_dependencies(update_ops):
            self.optimizer = tf.compat.v1.train.AdamOptimizer(learning_rate=self.learning_rate_tensor).minimize(self.cost, name='AdamMinimize')

        print("==============Node Name List==============")
        print("learning  rate tensor : ", self.learning_rate_tensor)
        print("Input Node Name : ", self.X)
        print("Output 4 Train Node Name : ", self.Y)
        print("Phase Node Name", self.keep_layer)
        print("Output Node Name (heatmap) : ", self.output)
        print("Output Node Name (sizemap) : ", self.size)
        print("Cost Function Node Name : ", self.cost)
        print("Run this operation for a train step            :", self.optimizer.name)
        print("==============Node Name List==============")

    def predict(self, x_test, keep_prop=False):
        return self.sess.run([self.output, self.size], feed_dict={self.X: x_test, self.keep_layer: keep_prop})

    def get_cost(self, x_test, y_test, y_size, keep_prop=False):
        # print(self.sess.run(self.output, feed_dict={self.X: x_test, self.keep_layer: keep_prop}))
        return self.sess.run(self.cost, feed_dict={self.X: x_test, self.Y: y_test, self.SIZE:y_size, self.keep_layer: keep_prop})

    def train(self, x_data, y_data, y_size, keep_prop=True, learn_rate=0.003):
        return self.sess.run(self.optimizer, feed_dict={self.X: x_data, self.Y: y_data, self.SIZE:y_size, self.keep_layer: keep_prop, self.learning_rate_tensor: learn_rate})