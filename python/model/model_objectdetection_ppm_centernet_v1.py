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


from util.helper import conv_block
from util.helper import bottlenect_block_v1
from util.helper import pyramid_pooling_block

# 0 cat , 1 dog,

class model_objectdetection_ppm_centernet_v1:

    def __init__(self, sess, class_count):
        self.sess = sess
        self.class_count = class_count
        self.up_sample_rate = 1
        self.feature_channels = 32
        #self.hourglass_channel = 32

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
        with tf.variable_scope('downsamples'):
            stage_1_1 = conv_block(self.X, conv_type='conv', filters=16, kernel_size=3, strides=2, training=self.keep_layer)
            stage_1_2 = conv_block(stage_1_1, conv_type='ds', filters=32, kernel_size=3, strides=2, training=self.keep_layer)
            stage_1_3 = conv_block(stage_1_2, conv_type='ds', filters=64, kernel_size=3, strides=2, training=self.keep_layer)



        with tf.variable_scope('feature_extraction'):
            feature1 = bottlenect_block_v1(inputs=stage_1_3, filters=64, kernel_size=3, upsample_rate=2, strides=2, repeat=2, training=self.keep_layer, name='residual1')
            feature2 = bottlenect_block_v1(inputs=feature1, filters=64, kernel_size=3, upsample_rate=2, strides=2, repeat=2, training=self.keep_layer, name='residual2')
            feature3 = bottlenect_block_v1(inputs=feature2, filters=32, kernel_size=3, upsample_rate=2, strides=1, repeat=2, training=self.keep_layer, name='residual3')


        with tf.variable_scope('pyramid_pooling'):
            pyramid = pyramid_pooling_block(feature3, kernel_size=32, input_width=32, input_height=32, bin_sizes=[2, 4, 6, 8])


        with tf.variable_scope('featurefuse'):
            feature_fuse_layer1  = conv_block(stage_1_3, conv_type='conv', filters=160, kernel_size=1, strides=1, training=self.keep_layer)
            print('test',feature_fuse_layer1)

            feature_fuse_layer2 = upsample_layer(pyramid, [128, 128])
            depthwise_filter = tf.compat.v1.get_variable('feature_fuse_layer2', [3, 3, 32 * 5, 1], initializer=tf.compat.v1.variance_scaling_initializer())
            feature_fuse_layer2 = tf.compat.v1.nn.depthwise_conv2d(input=feature_fuse_layer2, filter=depthwise_filter, strides=[1, 1, 1, 1], padding='SAME')
            print('feature_deptiwise conv=', feature_fuse_layer2)
            feature_fuse_layer2 = tf.compat.v1.layers.batch_normalization(feature_fuse_layer2, scale=True, center=True, momentum=0.9, training=self.keep_layer)
            feature_fuse_layer2 = tf.compat.v1.nn.relu(feature_fuse_layer2)
            feature_fuse_layer2 = tf.compat.v1.layers.conv2d(inputs=feature_fuse_layer2, filters=1, kernel_size=1, strides=1, padding='same', kernel_initializer=tf.compat.v1.variance_scaling_initializer())

            final_feature = feature_fuse_layer2 + feature_fuse_layer1
            final_feature = tf.compat.v1.layers.batch_normalization(final_feature, scale=True, center=True, momentum=0.9, training=self.keep_layer)
            final_feature = tf.compat.v1.nn.relu(final_feature)


        with tf.variable_scope('classifier'):
            classifiter = conv_block(final_feature, conv_type='ds', filters=64, kernel_size=3, strides=1, training=self.keep_layer)
            #classifiter = conv_block(classifiter, conv_type='ds', filters=64, kernel_size=3, strides=1, training=self.keep_layer)


        print("=== network structure ===")

        with tf.variable_scope("detector"):
            #self.cls = conv_elu_bn(feature_fuse_layer2, filters=self.feature_channels, training=self.keep_layer, kernel_size=3, strides=1, name='detector_convelu1')
            self.cls = conv(classifiter, filters=self.class_count, kernel_size=1, strides=1, name='detector_conv1')
            self.cls = tf.compat.v1.nn.sigmoid(self.cls, name="heatmap")

            #self.size = conv_elu_bn(feature_fuse_layer2, filters=self.feature_channels, training=self.keep_layer, kernel_size=3, strides=1, name='detector_convelu2')
            self.size = conv(classifiter, filters=2, kernel_size=1, strides=1, name='detector_conv2')
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