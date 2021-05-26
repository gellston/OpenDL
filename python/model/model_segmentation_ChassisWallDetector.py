import tensorflow as tf

from util.helper import atrus_conv
from util.helper import max_pool2d
from util.helper import upsample_layer


class model_segmentation_ChassisWallDetector:

    def __init__(self, sess, name):
        self.sess = sess
        self.name = name
        self._build_net()

    def _build_net(self):
        with tf.variable_scope(self.name):
            # placeholder 100x100 = 10000
            self.X = tf.placeholder(tf.float32, [None, 70, 70, 1], name='input')
            self.Y = tf.placeholder(tf.float32, [None, 70, 70, 1], name='output')
            self.learning_rate_tensor = tf.compat.v1.placeholder(tf.float32, shape=[], name='learning_rate')
            self.keep_layer = tf.placeholder(tf.bool, name='phase')

            print('=== network structure ===')
            encode1 = atrus_conv(self.X, kernel_size=3,  num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv1")
            encode2 = atrus_conv(encode1, kernel_size=3,  num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv2")
            max_pool1 = max_pool2d(encode2, pool_size=3, strides=2, name='max_pool1')
            #70x70

            encode3 = atrus_conv(max_pool1, kernel_size=3,  num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv3")
            encode4 = atrus_conv(encode3, kernel_size=3, num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv4")
            max_pool2 = max_pool2d(encode4, pool_size=3, strides=2, name='max_pool2')
            #35x35

            encode5 = atrus_conv(max_pool2, kernel_size=3,  num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv5")
            encode6 = atrus_conv(encode5, kernel_size=3, num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv6")
            max_pool3 = max_pool2d(encode6, pool_size=3, strides=2, name='max_pool3')
            #18x18

            encode7 = atrus_conv(max_pool3, kernel_size=3,  num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv7")
            encode8 = atrus_conv(encode7, kernel_size=3, num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv8")
            max_pool4 = max_pool2d(encode8, pool_size=3, strides=2, name='max_pool4')
            #9x9

            encode9 = atrus_conv(max_pool4, kernel_size=3,  num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv9")
            encode10 = atrus_conv(encode9, kernel_size=3, num_filter=128, dilate_rate=1, training=self.keep_layer, name="atrous_conv10")
            max_pool5 = max_pool2d(encode10, pool_size=3, strides=2, name='max_pool5')
            #5x5

            joint2 = atrus_conv(max_pool2, kernel_size=3, num_filter=32, dilate_rate=1, training=self.keep_layer, name="conv_joint2")
            joint3 = atrus_conv(max_pool3, kernel_size=3, num_filter=32, dilate_rate=1, training=self.keep_layer, name="conv_joint3")
            joint4 = atrus_conv(max_pool4, kernel_size=3, num_filter=32, dilate_rate=1, training=self.keep_layer, name="conv_joint4")
            joint5 = atrus_conv(max_pool5, kernel_size=3, num_filter=32, dilate_rate=1, training=self.keep_layer, name="conv_joint5")

            up1 = upsample_layer(joint2, [70, 70])
            up2 = upsample_layer(joint3, [70, 70])
            up3 = upsample_layer(joint4, [70, 70])
            up4 = upsample_layer(joint5, [70, 70])


            concat1 = tf.concat([ up1, up2, up3, up4], -1)
            channel_concat1 = atrus_conv(concat1, kernel_size=3, num_filter=32, dilate_rate=1, training=self.keep_layer, name="channel_concat1")
            channel_concat2 = atrus_conv(concat1, kernel_size=3, num_filter=32, dilate_rate=1, training=self.keep_layer, name="channel_concat2")
            channel_concat3 = atrus_conv(concat1, kernel_size=3, num_filter=32, dilate_rate=1, training=self.keep_layer, name="channel_concat3")
            channel_concat4 = atrus_conv(concat1, kernel_size=3, num_filter=32, dilate_rate=1, training=self.keep_layer, name="channel_concat4")

            concat2 = tf.concat([channel_concat1, channel_concat2, channel_concat3, channel_concat4], -1)


            pre_feature_conv = upsample_layer(concat2, [70, 70])
            encodeEnd1 = atrus_conv(pre_feature_conv, kernel_size=3,  num_filter=64, dilate_rate=1, training=self.keep_layer, name="encodeEnd1")
            encodeEnd2 = atrus_conv(encodeEnd1, kernel_size=3, num_filter=64, dilate_rate=1, training=self.keep_layer, name="encodeEnd2")



            feature_conv = tf.compat.v1.layers.conv2d(inputs=encodeEnd2, filters=1,
                                               kernel_size=[1, 1], padding="SAME",
                                               use_bias=False, strides=1,
                                               kernel_initializer=tf.compat.v1.variance_scaling_initializer())

            feature_relu = tf.compat.v1.nn.relu(feature_conv)

            last_feature = upsample_layer(feature_relu, [70, 70])

            self.output = tf.nn.sigmoid(last_feature)
            print('=== network structure ===')

            pre = tf.cast(self.output > 0.5, dtype=tf.float32)
            truth = tf.cast(self.Y > 0.5, dtype=tf.float32)
            inse = tf.reduce_sum(tf.multiply(pre, truth), axis=(1, 2, 3))  # AND
            union = tf.reduce_sum(tf.cast(tf.add(pre, truth) >= 1, dtype=tf.float32), axis=(1, 2, 3))  # OR
            batch_iou = (inse + 1e-5) / (union + 1e-5)
            self.accuracy = tf.reduce_mean(batch_iou, name='iou_coe1')


            self.cost = tf.reduce_mean(tf.nn.sigmoid_cross_entropy_with_logits(labels=self.Y, logits=last_feature))
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
