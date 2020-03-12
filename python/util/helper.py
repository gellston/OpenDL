import tensorflow as tf
import numpy as np

def inverted_bottle_neck(input, up_sample_rate, channels, shortcut=False, training=True, name="interted_bootle_neck"):
    with tf.variable_scope(name):
        stride = 1
        if shortcut == False :
            stride = 2


        filter_size = up_sample_rate * input.get_shape().as_list()[-1]
        net = tf.compat.v1.layers.conv2d(inputs=input, filters=filter_size, kernel_size=1, padding='same', kernel_initializer=tf.compat.v1.variance_scaling_initializer())
        net = tf.compat.v1.layers.batch_normalization(net, scale=True, momentum=0.9, training=training)
        net = tf.compat.v1.nn.relu6(net)

        depthwise_filter = tf.compat.v1.get_variable("depth_conv_w", [3, 3, filter_size, 1], initializer=tf.compat.v1.variance_scaling_initializer())
        net = tf.compat.v1.nn.depthwise_conv2d(input=net, filter=depthwise_filter, strides=[1, stride, stride, 1], padding='SAME')
        net = tf.compat.v1.layers.batch_normalization(net,scale=True, momentum=0.9, training=training)
        net = tf.compat.v1.nn.relu6(net)

        net = tf.compat.v1.layers.conv2d(inputs=net, filters=channels, kernel_size=3, padding='same', kernel_initializer=tf.compat.v1.variance_scaling_initializer())

        if input.get_shape().as_list()[-1] == channels and shortcut == True:
            net = tf.add(input, net)

        print('Inverted Bottleneck=', net)

        return net


def inverted_bottle_neck_elu(input, up_sample_rate, channels, shortcut=False, training=True, name="interted_bootle_neck_elu"):
    with tf.variable_scope(name):
        stride = 1
        if shortcut == False :
            stride = 2


        filter_size = up_sample_rate * input.get_shape().as_list()[-1]
        net = tf.compat.v1.layers.conv2d(inputs=input, filters=filter_size, kernel_size=1, padding='same', kernel_initializer=tf.compat.v1.variance_scaling_initializer())
        net = tf.compat.v1.layers.batch_normalization(net,  scale=True, momentum=0.9, training=training)
        net = tf.compat.v1.nn.elu(net)

        depthwise_filter = tf.compat.v1.get_variable("depth_conv_w", [3, 3, filter_size, 1], initializer=tf.compat.v1.variance_scaling_initializer())
        net = tf.compat.v1.nn.depthwise_conv2d(input=net, filter=depthwise_filter, strides=[1, stride, stride, 1], padding='SAME')
        net = tf.compat.v1.layers.batch_normalization(net, scale=True, momentum=0.9, training=training)
        net = tf.compat.v1.nn.elu(net)

        net = tf.compat.v1.layers.conv2d(inputs=net, filters=channels, kernel_size=1, padding='same', kernel_initializer=tf.compat.v1.variance_scaling_initializer())

        if input.get_shape().as_list()[-1] == channels and shortcut == True:
            net = tf.add(input, net)

        print('Inverted Bottleneck=', net)

        return net

def conv_relu6_bn(input, filters=3, kernel_size=3, strides=1, keep_layer=True):
    conv = tf.compat.v1.layers.conv2d(input, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, kernel_initializer=tf.compat.v1.variance_scaling_initializer(), padding="SAME")
    batch = tf.compat.v1.layers.batch_normalization(conv, scale=True,  momentum=0.9, training=keep_layer)
    relu = tf.compat.v1.nn.relu6(batch)
    print("conv_relu6_bn=", relu)
    return relu


def conv_relu6(input, filters=3, kernel_size=3, strides=1):
    conv = tf.compat.v1.layers.conv2d(input, scale=True,  filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, kernel_initializer=tf.compat.v1.variance_scaling_initializer(), padding="SAME")
    relu = tf.compat.v1.nn.relu6(conv)
    print("conv_relu6=", relu)
    return relu


def conv(input, filters=3, kernel_size=3, strides=1, name="conv"):
    with tf.variable_scope(name):
        conv = tf.compat.v1.layers.conv2d(input, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, kernel_initializer=tf.compat.v1.variance_scaling_initializer(), padding="SAME")
        print("conv=", conv)
        return conv


def deconv_relu6_bn(input, filters=3, kernel_size=3, strides=2, keep_layer=True):
    deconv = tf.compat.v1.layers.conv2d_transpose(input, filters=filters, kernel_size=kernel_size, strides=strides, padding='SAME', kernel_initializer=tf.compat.v1.variance_scaling_initializer())
    batch = tf.compat.v1.layers.batch_normalization(deconv, scale=True, momentum=0.9, training=keep_layer)
    relu = tf.compat.v1.nn.relu6(batch)
    print("deconv_relu6_bn=",relu)
    return relu



def creat_roiheatmap_ellipse(centern_roi, det_size_map):
    c_x, c_y = centern_roi
    sigma_x = ((det_size_map[1] - 1) * 0.5 - 1) * 0.3 + 0.8
    s_x = 2 * (sigma_x ** 2)
    sigma_y = ((det_size_map[0] - 1) * 0.5 - 1) * 0.3 + 0.8
    s_y = 2 * (sigma_y ** 2)
    X1 = np.arange(det_size_map[1])
    Y1 = np.arange(det_size_map[0])
    [X, Y] = np.meshgrid(X1, Y1)
    heatmap = np.exp(-(X - c_x) ** 2 / s_x - (Y - c_y) ** 2 / s_y)
    return heatmap






###############################################################################################################################################################################################

def conv_bn(input, filters=3, kernel_size=3, strides=1, training=True, name="conv_relu"):
    with tf.variable_scope(name):
        conv = tf.compat.v1.layers.conv2d(input, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, kernel_initializer=tf.compat.v1.variance_scaling_initializer(), padding="SAME")
        batch = tf.compat.v1.layers.batch_normalization(conv, scale=True, center=True, momentum=0.9, training=training)
        print("conv_elu=", batch)
        return batch


def conv_elu(input, filters=3, kernel_size=3, strides=1, name="conv_relu"):
    with tf.variable_scope(name):
        conv = tf.compat.v1.layers.conv2d(input, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, kernel_initializer=tf.compat.v1.variance_scaling_initializer(), padding="SAME")
        elu1 = tf.compat.v1.nn.elu(conv)
        print("conv_elu=", elu1)
        return elu1


def conv_elu_bn(input, filters=3, kernel_size=3, strides=1, training=True, name="conv_relu_bn"):
    with tf.variable_scope(name):
        conv = tf.compat.v1.layers.conv2d(input, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, kernel_initializer=tf.compat.v1.variance_scaling_initializer(), padding="SAME")
        batch = tf.compat.v1.layers.batch_normalization(conv, scale=True, center=True, momentum=0.9, training=training)
        elu1 = tf.compat.v1.nn.elu(batch)
        print("conv_elu_bn=", elu1)
        return elu1



def deconv_elu_bn(input, filters=3, kernel_size=3, strides=2,  training=True, name='deconv_elu_bn'):
    with tf.variable_scope(name):
        deconv = tf.compat.v1.layers.conv2d_transpose(input, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, padding='SAME', kernel_initializer=tf.compat.v1.variance_scaling_initializer())
        batch = tf.compat.v1.layers.batch_normalization(deconv, scale=True, center=True, momentum=0.9, training=training)
        relu = tf.compat.v1.nn.elu(batch)
        print("deconv_elu_bn=",relu)
        return relu

def deconv(input, filters=3, kernel_size=3, strides=2, name='deconv_elu_bn'):
    with tf.variable_scope(name):
        deconv = tf.compat.v1.layers.conv2d_transpose(input, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, padding='SAME', kernel_initializer=tf.compat.v1.variance_scaling_initializer());
        print("deconv=",deconv)
        return deconv

def upsample_layer(inputs, out_shape):
    new_width, new_height = out_shape[0], out_shape[1]
    # NOTE: here height is the first
    inputs = tf.image.resize_nearest_neighbor(inputs, (new_height, new_width), align_corners=True, name='upsampled')
    return inputs


def residual_block_elu(X_input, num_filter, training, name="residual_block_elu"):
    with tf.variable_scope(name):

        down_filters = num_filter / 2


        bm1 = tf.compat.v1.layers.batch_normalization(X_input, scale=True, center=True, momentum=0.9, training=training)
        relu1 = tf.compat.v1.nn.elu(bm1)
        conv1 = tf.compat.v1.layers.conv2d(inputs=relu1, filters=down_filters, kernel_size=[1, 1], padding="SAME", use_bias=False, strides=1, kernel_initializer=tf.compat.v1.variance_scaling_initializer())

        bm2 = tf.compat.v1.layers.batch_normalization(conv1, scale=True, center=True, momentum=0.9, training=training)
        relu2 = tf.compat.v1.nn.elu(bm2)
        conv2 = tf.compat.v1.layers.conv2d(inputs=relu2, filters=down_filters, kernel_size=[3, 3], padding="SAME", use_bias=False, strides=1, kernel_initializer=tf.compat.v1.variance_scaling_initializer())

        bm3 = tf.compat.v1.layers.batch_normalization(conv2, scale=True, center=True, momentum=0.9, training=training)
        relu3 = tf.compat.v1.nn.elu(bm3)
        conv3 = tf.compat.v1.layers.conv2d(inputs=relu3, filters=num_filter, kernel_size=[1, 1], padding="SAME", use_bias=False, strides=1, kernel_initializer=tf.compat.v1.variance_scaling_initializer())

        shortcut = tf.compat.v1.layers.conv2d(inputs=X_input, filters=num_filter, kernel_size=[1, 1], padding="SAME", use_bias=False, strides=1, kernel_initializer=tf.compat.v1.variance_scaling_initializer())
        summation = shortcut + conv3


        print("residual_block_elu=", summation)
        return summation
def hourglass_module(X_input, hourglass_filter, out_filter, training, name='hourglass_module'):
    with tf.variable_scope(name):
        stage8_1 = residual_block_elu(X_input, hourglass_filter, training=training, name=name + str(1))
        stage8_2 = residual_block_elu(stage8_1, hourglass_filter, training=training, name=name + str(2))
        stage8_3 = residual_block_elu(stage8_2, out_filter, training=training, name=name + str(3))
        print('hourglass_module=', stage8_3)
        return stage8_3


def separable_elu_bn(input, filters=3, kernel_size=3, strides=1, dilation_rate=1, training=True, name="conv_relu_bn"):
    with tf.variable_scope(name):
        conv = tf.compat.v1.layers.separable_conv2d(input, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, depthwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                                                                                                      pointwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                                                                                                      dilation_rate=dilation_rate, padding="SAME")
        batch = tf.compat.v1.layers.batch_normalization(conv, scale=True, center=True,  momentum=0.9, training=training)
        elu1 = tf.compat.v1.nn.elu(batch)
        print("separable_elu_bn=", elu1)
        return elu1

def max_pool2d(input, pool_size=3, strides=1, name='max_pool'):
    with tf.variable_scope(name):
        max = tf.compat.v1.layers.max_pooling2d(input, pool_size=pool_size, strides=strides, padding='SAME')
        print('max_pool=', max)
        return max


def max_pool3d(input, pool_size=3, strides=1, name='max_pool'):
    with tf.variable_scope(name):
        max = tf.compat.v1.layers.max_pooling3d(input, pool_size=pool_size, strides=strides, padding='SAME')
        print('max_pool=', max)
        return max
###############################################################################################################################################################################################







################################################################################
################################################################################
################################################################################

def conv_block(inputs, conv_type, filters, kernel_size, strides, padding='same', training=True, relu=True):
    if conv_type == 'ds':
        x = tf.compat.v1.layers.separable_conv2d(inputs, filters=filters, kernel_size=kernel_size, strides=strides, use_bias=False, depthwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                                                                                                      pointwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                                                                                                      padding=padding)
    else:
        x = tf.compat.v1.layers.conv2d(inputs=inputs, filters=filters, kernel_size=kernel_size, padding=padding, kernel_initializer=tf.compat.v1.variance_scaling_initializer())

    x = tf.compat.v1.layers.batch_normalization(x, scale=True, center=True,  momentum=0.9, training=training)
    if(relu):
        x = tf.compat.v1.nn.elu(x)

    print('conv_block=',x)
    return x



def _res_bottlenect(inputs, filters, kernel_size, upsample_rate, strides, training=True, route=False, name='_res_bottlenect'):


    tchannel = upsample_rate * inputs.get_shape().as_list()[-1]

    x = conv_block(inputs, 'conv', tchannel, kernel_size=1, strides=1)

    depthwise_filter = tf.compat.v1.get_variable(name, [kernel_size, kernel_size, tchannel, 1], initializer=tf.compat.v1.variance_scaling_initializer())
    x = tf.compat.v1.nn.depthwise_conv2d(input=x, filter=depthwise_filter, strides=[1, strides, strides, 1], padding='SAME')
    x = tf.compat.v1.layers.batch_normalization(x, scale=True, center=True, momentum=0.9, training=training)
    x = tf.compat.v1.nn.relu(x)

    x = conv_block(x, 'conv', filters=filters, kernel_size=1,  strides=1, padding='same', relu=False)

    if route:
        x = inputs + x

    return x


def bottlenect_block_v1(inputs, filters, kernel_size, upsample_rate, strides, training, repeat, name='bottlenect_block_v1'):

    x = _res_bottlenect(inputs, filters, kernel_size=kernel_size, upsample_rate=upsample_rate, strides=strides, name= name +'before_route')
    for i in range(0, repeat):
        x = _res_bottlenect(x, filters, kernel_size=kernel_size, upsample_rate=upsample_rate, strides=1, training=training, route=True, name=name +'repeat_residual_' + str(i))

    print('bottle_block_v1=', x)

    return x


def pyramid_pooling_block(input_tensor, kernel_size, input_width, input_height,  bin_sizes):

    concat_list = [input_tensor]

    for bin_size in bin_sizes:
        x = tf.compat.v1.layers.average_pooling2d(inputs=input_tensor, pool_size=(input_width / bin_size, input_height / bin_size), strides=(input_width / bin_size, input_height / bin_size))
        x = tf.compat.v1.layers.conv2d(inputs=x, filters=kernel_size, kernel_size=3, strides=2, padding='same', kernel_initializer=tf.compat.v1.variance_scaling_initializer())
        x = tf.image.resize_nearest_neighbor(x, (input_height, input_width), align_corners=True)
        concat_list.append(x)

    concat = tf.concat(concat_list, -1)
    print('pyramid_pooling=', concat)
    return concat


################################################################################
################################################################################
################################################################################


def focal_loss(pred, gt):
    ''' Modified focal loss. Exactly the same as CornerNet.
        Runs faster and costs a little bit more memory
      Arguments:
        pred (batch,h,w,c)
        gt_regr (batch,h,w,c)
    '''
    pos_inds = tf.cast(tf.equal(gt, 1.0), dtype=tf.float32)
    neg_inds = 1.0 - pos_inds
    neg_weights = tf.pow(1.0 - gt, 4.0)

    pred = tf.clip_by_value(pred, 1e-6, 1.0 - 1e-6)
    pos_loss = tf.log(pred) * tf.pow(1.0 - pred, 2.0) * pos_inds
    neg_loss = tf.log(1.0 - pred) * tf.pow(pred, 2.0) * neg_weights * neg_inds

    num_pos = tf.reduce_sum(pos_inds)
    pos_loss = tf.reduce_sum(pos_loss)
    neg_loss = tf.reduce_sum(neg_loss)

    loss = - (pos_loss + neg_loss) / num_pos


    return loss


def reg_l1_loss(pred, gt):
  '''
  :param pred: (batch,h,w,c)
  :param gt: (batch,h,w,c)
  :return:
  '''
  mask = tf.cast(tf.greater(gt, 0.0), dtype=tf.float32)
  num_pos = (tf.reduce_sum(mask) + tf.convert_to_tensor(1e-4))
  loss = tf.abs(pred - gt) * mask
  loss = tf.reduce_sum(loss) / num_pos
  return loss




#### Gaussian Heatmap Functions
def multivariate_gaussian(pos, mu, Sigma):
    """
    Return the multivariate Gaussian distribution on array pos.
    pos is an array constructed by packing the meshed arrays of variables
    x_1, x_2, x_3, ..., x_k into its _last_ dimension.
    """
    n = mu.shape[0]
    Sigma_det = np.linalg.det(Sigma)
    Sigma_inv = np.linalg.inv(Sigma)
    N = np.sqrt((2*np.pi)**n * Sigma_det)
    # This einsum call calculates (x-mu)T.Sigma-1.(x-mu) in a vectorized
    # way across all the input variables.
    fac = np.einsum('...k,kl,...l->...', pos-mu, Sigma_inv, pos-mu)
    return np.exp(-fac / 2) / N

def gaussian_heat_map(image_width, image_height, center_x, center_y, gauss_width, gauss_height):
    X = np.linspace(0, image_width, image_height)
    Y = np.linspace(0, image_width, image_height)
    X, Y = np.meshgrid(X, Y)

    mu = np.array([center_x, center_y])
    Sigma = np.array([[gauss_width ,0.], [0.,gauss_height]])

    # Pack X and Y into a single 3-dimensional array
    pos = np.empty(X.shape + (2,))
    pos[:, :, 0] = X
    pos[:, :, 1] = Y

    Z = multivariate_gaussian(pos, mu, Sigma)
    return Z

def creat_roiheatmap_ellipse(centern_roi, det_size_map):
    c_x, c_y = centern_roi
    sigma_x = ((det_size_map[1] - 1) * 0.5 - 1) * 0.3 + 0.8
    s_x = 2 * (sigma_x ** 2)
    sigma_y = ((det_size_map[0] - 1) * 0.5 - 1) * 0.3 + 0.8
    s_y = 2 * (sigma_y ** 2)
    X1 = np.arange(det_size_map[1])
    Y1 = np.arange(det_size_map[0])
    [X, Y] = np.meshgrid(X1, Y1)
    heatmap = np.exp(-(X - c_x) ** 2 / s_x - (Y - c_y) ** 2 / s_y)
    return heatmap


def extract_box_from_featuremaps(image_width, image_height, scale, gaussian_heatmaps, size_maps, desire_pobability):
    box_list = []
    count = 0
    for y in range(1, image_height - 1):
        if count == 100:
            break
        for x in range(1, image_width - 1):
            if count == 100:
                break
            center_value = gaussian_heatmaps[y, x]
            if center_value > desire_pobability:
                if np.max(gaussian_heatmaps[y-1:y+1,x-1:x+1] == center_value):
                    count +=1
                    width = size_maps[y, x, 0]
                    height = size_maps[y, x, 1]
                    start_x = int((x - width / 2)*scale)
                    start_y = int((y - height / 2)*scale)
                    end_x = int((x + width / 2)*scale)
                    end_y = int((y + height / 2)*scale)
                    box_list.append([start_x, start_y, end_x, end_y])

    return box_list


def extract_box_from_featuremaps_with_score(image_width, image_height, scale, gaussian_heatmaps, size_maps, desire_pobability):
    box_list = []
    count = 0
    for y in range(1, image_height - 1):
        if count == 100:
            break
        for x in range(1, image_width - 1):
            if count == 100:
                break
            center_value = gaussian_heatmaps[y, x]
            if center_value > desire_pobability:
                if np.max(gaussian_heatmaps[y-1:y+1,x-1:x+1] == center_value):
                    count +=1
                    width = size_maps[y, x, 0]
                    height = size_maps[y, x, 1]
                    start_x = int((x - width / 2)*scale)
                    start_y = int((y - height / 2)*scale)
                    end_x = int((x + width / 2)*scale)
                    end_y = int((y + height / 2)*scale)
                    box_list.append([center_value, start_x, start_y, end_x, end_y])

    return box_list


def IOU(boxA, boxB):
    # determine the (x, y)-coordinates of the intersection rectangle
    xA = max(boxA[0], boxB[0])
    yA = max(boxA[1], boxB[1])
    xB = min(boxA[2], boxB[2])
    yB = min(boxA[3], boxB[3])

    # compute the area of intersection rectangle
    interArea = max(0, xB - xA + 1) * max(0, yB - yA + 1)

    # compute the area of both the prediction and ground-truth
    # rectangles
    boxAArea = (boxA[2] - boxA[0] + 1) * (boxA[3] - boxA[1] + 1)
    boxBArea = (boxB[2] - boxB[0] + 1) * (boxB[3] - boxB[1] + 1)

    # compute the intersection over union by taking the intersection
    # area and dividing it by the sum of prediction + ground-truth
    # areas - the interesection area
    iou = interArea / float(boxAArea + boxBArea - interArea)

    # return the intersection over union value
    return iou


def filter_overlap_boxes(box_list, desire_probability):
    arranged_box = sorted(box_list, key=lambda x: -x[0])
    #print('arranged list = ', arranged_box)
    for index1 in range(0, len(arranged_box) - 1):
        temp_box = arranged_box[index1]
        if temp_box[0] == 0 :
            continue
        currentBox = [temp_box[1], temp_box[2], temp_box[3], temp_box[4]]
        for index2 in range(index1 + 1, len(arranged_box)):
            temp_box = arranged_box[index2]
            compareBox = [temp_box[1], temp_box[2], temp_box[3], temp_box[4]]
            if compareBox[0] == 0:
                continue

            score = IOU(currentBox, compareBox)

            if score > desire_probability :
                #print('filtered score=', score)
                arranged_box[index2][0] = 0

    finalBox = []
    for box in arranged_box:
        if box[0] > 0:
            finalBox.append([box[1], box[2], box[3], box[4]])

    return finalBox


def mAP_calculation(label_list, predict_list, desire_probability):
    count = 0
    for label_box in label_list:
        for predict_box in predict_list:
            score = IOU(label_box, predict_box)
            if score >= desire_probability :
                #print('passed iou= ', score)
                count = count + 1
                break

    if count == 0:
        return 0
    else:
        return count / len(label_list)



########################################## Segmentation Layer #####################################################################

def transition_up(name, x, filters):
    x = tf.compat.v1.layers.conv2d_transpose(x,
                                      filters=filters,
                                      kernel_size=[3, 3],
                                      strides=[2, 2],
                                      use_bias=False,
                                      padding='SAME',
                                      activation=None,
                                      kernel_initializer=tf.compat.v1.variance_scaling_initializer(),
                                      name=name+'_trans_conv3x3')
    return x


def residual_layer(name, input, filters, is_batch_norm):
    encode = tf.compat.v1.layers.batch_normalization(input,
                                                     scale=True,
                                                     center=False,
                                                     momentum=0.9,
                                                     training=is_batch_norm)
    encode = tf.compat.v1.nn.leaky_relu(encode)
    encode = tf.compat.v1.layers.separable_conv2d(encode,
                                                  filters=filters,
                                                  kernel_size=[3, 3],
                                                  strides=[1, 1],
                                                  use_bias=False,
                                                  padding='SAME',
                                                  activation=None,
                                                  pointwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                  depthwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                                  name=name + '_conv3x3')
    return encode

def residual_block(name, x, filters, is_batch_norm):
    with tf.name_scope("residual_block_" + name):
        identity = x
        x = residual_layer(name + 'layer1', x, filters, is_batch_norm)
        x = residual_layer(name + 'layer2', x, filters, is_batch_norm)
        output = x + identity
    return output

def transition_down_expandChannelDouble(name, input, filters, is_batch_norm):
    x = tf.compat.v1.nn.max_pool(input, [1, 3, 3, 1], [1, 2, 2, 1], padding='SAME', name=name + '_maxpool3x3')
    x = tf.layers.separable_conv2d(x,
                                   filters=filters,
                                   kernel_size=[3, 3],
                                   strides=[1, 1],
                                   use_bias=False,
                                   padding='SAME',
                                   activation=None,
                                   pointwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                   depthwise_initializer=tf.compat.v1.variance_scaling_initializer(),
                                   name=name + '_conv1x1')

    x = tf.compat.v1.layers.batch_normalization(x, scale=True, momentum=0.9, center=True, training=is_batch_norm)
    x = tf.compat.v1.nn.relu(x, name=name + 'relu')

    return x


########################################## Anomaly Layer #####################################################################

def TU_ANO(name, input, filters, kernel_size=[3, 3], strides=[2, 2], is_batch_norm=True, padding='SAME'):
    with tf.variable_scope(name):
        x = tf.compat.v1.layers.conv2d_transpose(input,
                                          filters=filters,
                                          kernel_size=kernel_size,
                                          strides=strides,
                                          use_bias=False,
                                          padding=padding,
                                          activation=None,
                                          kernel_initializer=tf.compat.v1.variance_scaling_initializer(),
                                          name=name+'_trans_conv3x3')

        #deconv = tf.compat.v1.layers.batch_normalization(x,
        #                                                 scale=True,
        #                                                 center=True,
        #                                                 momentum=0.9,
        #                                                 training=is_batch_norm)

        deconv = tf.compat.v1.nn.leaky_relu(x)
        print('TU_ANO', deconv)
        return deconv

def TU_ANO_RELU(name, input, filters, kernel_size=[3, 3], strides=[2, 2], is_batch_norm=True, padding='SAME'):
    with tf.variable_scope(name):
        x = tf.compat.v1.layers.conv2d_transpose(input,
                                          filters=filters,
                                          kernel_size=kernel_size,
                                          strides=strides,
                                          use_bias=False,
                                          padding=padding,
                                          activation=None,
                                          kernel_initializer=tf.compat.v1.variance_scaling_initializer(),
                                          name=name+'_trans_conv3x3')

        #deconv = tf.compat.v1.layers.batch_normalization(x,
        #                                                 scale=True,
        #                                                 center=True,
        #                                                 momentum=0.9,
        #                                                 training=is_batch_norm)

        deconv = tf.compat.v1.nn.leaky_relu(x)
        print('TU_ANO', deconv)
        return deconv


def CONV_ANO(name, input, filters, kernel_size=[3, 3], strides=[1, 1], is_batch_norm=True, padding='SAME'):
    with tf.variable_scope(name):
        encode = tf.compat.v1.layers.conv2d(input,
                                          filters=filters,
                                          kernel_size=kernel_size,
                                          strides=strides,
                                          use_bias=False,
                                          padding=padding,
                                          activation=None,
                                          kernel_initializer=tf.compat.v1.variance_scaling_initializer(),
                                          name=name + '_conv3x3')

        #conv = tf.compat.v1.layers.batch_normalization(encode,
        #                                                 scale=True,
        #                                                 center=True,
        #                                                 momentum=0.9,
        #                                                 training=is_batch_norm)

        conv = tf.compat.v1.nn.leaky_relu(encode)
        print('CONV_ANO', conv)
        return conv


def CONV_ANO_RELU(name, input, filters, kernel_size=[3, 3], strides=[1, 1], is_batch_norm=True, padding='SAME'):
    with tf.variable_scope(name):
        encode = tf.compat.v1.layers.conv2d(input,
                                          filters=filters,
                                          kernel_size=kernel_size,
                                          strides=strides,
                                          use_bias=False,
                                          padding=padding,
                                          activation=None,
                                          kernel_initializer=tf.compat.v1.variance_scaling_initializer(),
                                          name=name + '_conv3x3_relu')

        #conv = tf.compat.v1.layers.batch_normalization(encode,
        #                                                 scale=True,
        #                                                 center=True,
        #                                                 momentum=0.9,
        #                                                 training=is_batch_norm)

        conv = tf.compat.v1.nn.leaky_relu(encode)
        print('CONV_ANO', conv)
        return conv

def UPSAMPLING(name, input, multiple=2):
    with tf.variable_scope(name):
        height = input.get_shape().as_list()[1]
        width = input.get_shape().as_list()[2]
        x = tf.image.resize_nearest_neighbor(input, size=(height * multiple, width * multiple))
        print('UPSAMPLING', x)
        return x