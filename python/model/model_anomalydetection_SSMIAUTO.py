import tensorflow as tf

from util.helper import TU_ANO
from util.helper import CONV_ANO
from util.helper import CONV_ANO_RELU



class model_anomalydetection_SSMIAUTO:

    def __init__(self, sess, name):
        self.sess = sess
        self.name = name

        with tf.variable_scope(name):
            self._build_net()



    def _build_net(self):

        print('=== network structure ===')

        self.X = tf.placeholder(tf.float32, [None, 256, 256, 1], name='RealTarget')
        self.learning_rate_tensor = tf.compat.v1.placeholder(tf.float32, shape=[], name='learning_rate')
        self.keep_layer = tf.placeholder(tf.bool, name='phase')


        conv1 = CONV_ANO(name='conv1', input=self.X, filters=64, kernel_size=[4, 4], strides=[2, 2], #
                        is_batch_norm=self.keep_layer)

        conv2 = CONV_ANO(name='conv2', input=conv1, filters=64, kernel_size=[4, 4], strides=[2, 2], #
                        is_batch_norm=self.keep_layer)

        conv3 = CONV_ANO(name='conv3', input=conv2, filters=64, kernel_size=[4, 4], strides=[1, 1], #
                        is_batch_norm=self.keep_layer)

        conv4 = CONV_ANO(name='conv4', input=conv3, filters=128, kernel_size=[4, 4], strides=[2, 2],#
                        is_batch_norm=self.keep_layer)

        conv5 = CONV_ANO(name='conv5', input=conv4, filters=128, kernel_size=[4, 4], strides=[1, 1], #
                        is_batch_norm=self.keep_layer)

        conv6 = CONV_ANO(name='conv6', input=conv5, filters=256, kernel_size=[4, 4], strides=[2, 2], #
                        is_batch_norm=self.keep_layer)

        conv7 = CONV_ANO(name='conv7', input=conv6, filters=128, kernel_size=[3, 3], strides=[1, 1], #
                        is_batch_norm=self.keep_layer)

        conv8 = CONV_ANO(name='conv8', input=conv7, filters=64, kernel_size=[3, 3], strides=[1, 1], #
                        is_batch_norm=self.keep_layer)

        conv9 = CONV_ANO(name='conv9', input=conv8, filters=32, kernel_size=[6, 6], strides=[1, 1],
                        is_batch_norm=self.keep_layer, padding='VALID')  ### Latent Z Conv Part !!!!!!!!!!!!!!!!!!!!

        center = conv9 #### Latent Z Part!!!!!!!!!!!!!!!
        #print(center)

        deconv = TU_ANO(name='deconv1', input=center, filters=64, kernel_size=[6, 6], strides=[1, 1], #
                        is_batch_norm=self.keep_layer, padding='VALID')

        deconv = deconv + conv8
        deconv = TU_ANO(name='deconv2', input=deconv, filters=128, kernel_size=[3, 3], strides=[1, 1],#
                        is_batch_norm=self.keep_layer)

        deconv = deconv + conv7
        deconv = TU_ANO(name='deconv3', input=deconv, filters=256, kernel_size=[3, 3], strides=[1, 1],#
                        is_batch_norm=self.keep_layer)

        deconv = deconv + conv6
        deconv = TU_ANO(name='deconv4', input=deconv, filters=128, kernel_size=[4, 4], strides=[2, 2],#
                        is_batch_norm=self.keep_layer)

        deconv = deconv + conv5
        deconv = TU_ANO(name='deconv5', input=deconv, filters=128, kernel_size=[4, 4], strides=[1, 1],#
                        is_batch_norm=self.keep_layer)

        deconv = deconv + conv4
        deconv = TU_ANO(name='deconv6', input=deconv, filters=64, kernel_size=[4, 4], strides=[2, 2], #
                        is_batch_norm=self.keep_layer)

        deconv = TU_ANO(name='deconv7', input=deconv, filters=64, kernel_size=[4, 4], strides=[1, 1], #
                        is_batch_norm=self.keep_layer)

        deconv = TU_ANO(name='deconv8', input=deconv, filters=64, kernel_size=[4, 4], strides=[2, 2],
                        is_batch_norm=self.keep_layer)

        deconv = TU_ANO(name='deconv9', input=deconv, filters=1, kernel_size=[4, 4], strides=[2, 2],
                        is_batch_norm=self.keep_layer)

        #deconv = CONV_ANO(name='end_filter', input=deconv, filters=1, kernel_size=[4, 4], strides=[1, 1],
        #                                is_batch_norm=self.keep_layer)

        self.pure_conv = deconv
        deconv = tf.nn.sigmoid(deconv)

        self.output = deconv



        self.ssmi_loss  = tf.reduce_mean(tf.square(1 - (tf.image.ssim(self.X, self.output, max_val=1) + 1) / 2))
        self.custom_loss = tf.reduce_mean(tf.square(1 - tf.image.ssim(self.X, self.output, max_val=1)))
        self.l1_loss = tf.reduce_mean(tf.abs(self.X - self.output))
        self.l2_loss = tf.losses.mean_squared_error(self.X, self.output)
        self.cross_entropy = tf.reduce_mean(tf.nn.sigmoid_cross_entropy_with_logits(logits=self.pure_conv, labels=self.X))
        self.loss = self.ssmi_loss + self.l1_loss

        print('=== network structure ===')

        update_ops = tf.get_collection(tf.GraphKeys.UPDATE_OPS)
        with tf.control_dependencies(update_ops):
            self.optimizer = tf.train.AdamOptimizer(learning_rate=self.learning_rate_tensor).minimize(self.loss)

        #tf.compat.v1.train.AdamOptimizer(learning_rate=self.learning_rate_tensor).minimize(
        #    self.cost, name='AdamMinimize')


        self.accuracy = tf.reduce_mean(tf.image.ssim(self.X, self.output, max_val=1))


        print("==============Node Name List==============")
        print("learning  rate tensor : ", self.learning_rate_tensor)
        print("Input Node Name : ", self.X)
        print("Phase Node Name", self.keep_layer)
        print("Accuracy Node Name : ", self.accuracy)
        print("Output Node Name : ", self.output)
        print("Train Node Name : ", self.optimizer.name)
        print("==============Node Name List==============")


    def reconstruct(self, x_test, keep_prop=False):
        return self.sess.run(self.output, feed_dict={self.X: x_test, self.keep_layer: keep_prop})

    def train(self, x_data, keep_prop=True, learning_rate=0.003):
        return self.sess.run([self.optimizer, self.loss], feed_dict={self.X: x_data, self.keep_layer: keep_prop, self.learning_rate_tensor:learning_rate})

    def get_accuracy(self, x_test, keep_prop=False):
        return self.sess.run(self.accuracy, feed_dict={self.X: x_test, self.keep_layer: keep_prop})




