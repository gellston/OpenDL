import tensorflow as tf

from util.helper import focal_loss

from util.helper import inverted_bottle_neck_elu
from util.helper import deconv_elu_bn
from util.helper import conv_elu_bn
from util.helper import conv_elu
from util.helper import conv
from util.helper import reg_l1_loss

# 0 cat , 1 dog,

class model_objectdetection_centernet:

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


        with tf.variable_scope('stage_4'):
            out_2 = conv_elu_bn(self.X, filters=24, kernel_size=3, strides=2, training=self.keep_layer, name="conv_elu1")
            out_4 = conv_elu_bn(out_2, filters=24, kernel_size=3, strides=2, training=self.keep_layer, name="conv_elu2")

        with tf.variable_scope('stage_8'):
            out_5 = inverted_bottle_neck_elu(out_4, self.up_sample_rate, 48, shortcut=False, training=self.keep_layer, name="invert1_")
            for i in range(4):
                out_5 = inverted_bottle_neck_elu(out_5, self.up_sample_rate, 48, shortcut=True, training=self.keep_layer, name="invert1_" + str(i))


        with tf.variable_scope('stage_16'):
            out_6 = inverted_bottle_neck_elu(out_5, self.up_sample_rate, 96, shortcut=False, training=self.keep_layer, name="invert2_")
            for i in range(8):
                out_6 = inverted_bottle_neck_elu(out_6, self.up_sample_rate, 96, shortcut=True, training=self.keep_layer, name="invert2_" + str(i))


        with tf.variable_scope('stage_32'):
            out_7 = inverted_bottle_neck_elu(out_6, self.up_sample_rate, 192, shortcut=False, training=self.keep_layer, name="invert3_")
            for i in range(4):
                out_7 = inverted_bottle_neck_elu(out_7, self.up_sample_rate, 192, shortcut=True, training=self.keep_layer, name="invert3_" + str(i))


        with tf.variable_scope('feature_map_fuse'):
            deconv1 = deconv_elu_bn(out_7, filters=self.feature_channels, kernel_size=4, strides=2, training=self.keep_layer, name='deconv1')
            out_8 = conv_elu_bn(out_6, filters=self.feature_channels, kernel_size=1, strides=1, training=self.keep_layer, name="conv_elu3")
            fuse1 = deconv1 + out_8

            deconv2 = deconv_elu_bn(fuse1, filters=self.feature_channels, kernel_size=4, strides=2, training=self.keep_layer, name='deconv2')
            out_9 = conv_elu_bn(out_5, filters=self.feature_channels, kernel_size=1, strides=1, training=self.keep_layer, name="conv_elu4")
            fuse2 = deconv2 + out_9

            deconv3 = deconv_elu_bn(fuse2, filters=self.feature_channels, kernel_size=4, strides=2, training=self.keep_layer, name='deconv3')
            out_10 = conv_elu_bn(out_4, filters=self.feature_channels, kernel_size=1, strides=1, training=self.keep_layer, name="conv_elu5")
            fuse3 = deconv3 + out_10


        print("=== network structure ===")

        with tf.variable_scope("detector"):
            self.cls = conv_elu(fuse3, filters=self.feature_channels, kernel_size=3, strides=1, name='detector_convelu1')
            self.cls = conv(self.cls, filters=self.class_count, kernel_size=1, strides=1, name='detector_conv1')
            self.cls = tf.compat.v1.nn.sigmoid(self.cls, name="heatmap")


            self.size = conv_elu(fuse3, filters=self.feature_channels, kernel_size=3, strides=1, name='detector_convelu2')
            self.size = conv(self.size, filters=2, kernel_size=1, strides=1, name='detector_conv2')
            self.size = tf.compat.v1.nn.relu(self.size, name='sizemap')


            print("heatmap sigmoid=", self.cls)


        self.output = self.cls;
        print("=== network structure ===")



        pre = tf.cast(self.output > 0.5, dtype=tf.float32)
        truth = tf.cast(self.Y > 0.5, dtype=tf.float32)
        inse = tf.reduce_sum(tf.multiply(pre, truth), axis=(1, 2, 3))  # AND
        union = tf.reduce_sum(tf.cast(tf.add(pre, truth) >= 1, dtype=tf.float32), axis=(1, 2, 3))  # OR
        batch_iou = (inse + 1e-5) / (union + 1e-5)
        self.accuracy = tf.reduce_mean(batch_iou, name='iou_coe1')


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
        print("Accuracy Node Name : ", self.accuracy)
        print("Output Node Name (Heatmap) : ", self.cls)
        print("Output Node Name (Sizemap) : ", self.size)
        print("Cost Function Node Name : ", self.cost)
        print("Run this operation for a train step            :", self.optimizer.name)
        print("==============Node Name List==============")

    def predict(self, x_test, keep_prop=False):
        return self.sess.run([self.output, self.size], feed_dict={self.X: x_test, self.keep_layer: keep_prop})

    def get_cost_accuracy(self, x_test, y_test, y_size, keep_prop=False):
        # print(self.sess.run(self.output, feed_dict={self.X: x_test, self.keep_layer: keep_prop}))
        return self.sess.run([self.accuracy, self.cost], feed_dict={self.X: x_test, self.Y: y_test, self.SIZE:y_size, self.keep_layer: keep_prop})

    def train(self, x_data, y_data, y_size, keep_prop=True, learn_rate=0.003):
        return self.sess.run(self.optimizer, feed_dict={self.X: x_data, self.Y: y_data, self.SIZE:y_size, self.keep_layer: keep_prop, self.learning_rate_tensor: learn_rate})