import tensorflow as tf

from util.helper import focal_loss

from util.helper import deconv_elu_bn
from util.helper import conv_elu_bn
from util.helper import conv_elu
from util.helper import conv
from util.helper import reg_l1_loss
from util.helper import separable_elu_bn

# 0 cat , 1 dog,

class model_objectdetection_atrous_centernet:

    def __init__(self, sess, class_count):
        self.sess = sess
        self.class_count = class_count
        self.up_sample_rate = 1
        self.feature_channels = 256

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


        ## 128x 128
        with tf.variable_scope('stage_4'):
            stage_4_1 = separable_elu_bn(self.X, filters=24, kernel_size=3, strides=2, dilation_rate=1, training=self.keep_layer, name='stage_4_1')
            stage_4_2 = separable_elu_bn(stage_4_1, filters=24, kernel_size=3, strides=2, dilation_rate=1, training=self.keep_layer, name='stage_4_2')

        ## 64x64
        with tf.variable_scope('stage_8'):
            stage_8_1 = separable_elu_bn(stage_4_2,  filters=48, kernel_size=3, strides=1, dilation_rate=1, training=self.keep_layer, name='stage_8_1')
            stage_8_2 = separable_elu_bn(stage_8_1, filters=48, kernel_size=3, strides=2, dilation_rate=1, training=self.keep_layer, name='stage_8_2')

        ## 32x32
        with tf.variable_scope('stage_16'):
            stage_16_1 = separable_elu_bn(stage_8_2,  filters=96, kernel_size=3, strides=1, dilation_rate=1, training=self.keep_layer, name='stage_16_1')
            stage_16_2 = separable_elu_bn(stage_16_1, filters=96, kernel_size=3, strides=2, dilation_rate=1, training=self.keep_layer, name='stage_16_2')

        ## Feature Extraction 32x32
        with tf.variable_scope('feature_extraction'):
            extraction1 = separable_elu_bn(stage_16_2, filters=192, kernel_size=3, strides=1, dilation_rate=1, training=self.keep_layer, name='separable_1')
            extraction2 = separable_elu_bn(stage_16_2, filters=192, kernel_size=3, strides=1, dilation_rate=2, training=self.keep_layer, name='separable_2')
            extraction3 = separable_elu_bn(stage_16_2, filters=192, kernel_size=3, strides=1, dilation_rate=3, training=self.keep_layer, name='separable_3')
            extraction4 = separable_elu_bn(stage_16_2, filters=192, kernel_size=3, strides=1, dilation_rate=6, training=self.keep_layer, name='separable_4')

            concat_features1 = tf.concat((extraction1, extraction2, extraction3, extraction4, stage_16_2), axis=3)
            all_in_one_feature = separable_elu_bn(concat_features1, filters=self.feature_channels, kernel_size=1, strides=1, dilation_rate=1, training=self.keep_layer, name='all_in_one_feature')

        ## deconv 64x64
        with tf.variable_scope('feature_convolution1'):
            deconv1 = deconv_elu_bn(all_in_one_feature, filters=self.feature_channels, kernel_size=3, strides=2, training=self.keep_layer, name='deconv1')
            mirror1 = separable_elu_bn(stage_8_2, filters=self.feature_channels, kernel_size=3, strides=1, dilation_rate=2, training=self.keep_layer, name='mirror1')
            summation_feature1 = mirror1 + deconv1
            print(summation_feature1)


        ## deconv 128, 128
        with tf.variable_scope('feature_convolution2'):
            deconv2 = deconv_elu_bn(summation_feature1, filters=self.feature_channels, kernel_size=3, strides=2, training=self.keep_layer, name='deconv2')
            mirror2 = separable_elu_bn(stage_4_2, filters=self.feature_channels, kernel_size=3, strides=1, dilation_rate=3, training=self.keep_layer, name='mirror2')
            summation_feature2 = mirror2 + deconv2
            print(summation_feature2)




        print("=== network structure ===")

        with tf.variable_scope("detector"):
            self.cls = conv_elu(summation_feature2, filters=self.feature_channels, kernel_size=3, strides=1, name='detector_convelu1')
            self.cls = conv(self.cls, filters=self.class_count, kernel_size=1, strides=1, name='detector_conv1')
            self.cls = tf.compat.v1.nn.sigmoid(self.cls, name="heatmap")

            #self.L2 = tf.reduce_mean((self.cls - self.Y ) * (self.cls - self.Y ))


            self.size = conv_elu(summation_feature2, filters=self.feature_channels, kernel_size=3, strides=1, name='detector_convelu2')
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
        #print("Accuracy Node Name : ", self.accuracy)
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